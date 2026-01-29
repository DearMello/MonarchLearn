using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using MonarchLearn.Application.DTOs.Common;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Domain.Entities.Courses;
using MonarchLearn.Domain.Exceptions;
using MonarchLearn.Application.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MonarchLearn.Application.Services
{
    public class SkillService : ISkillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SkillService> _logger;
        private readonly IDistributedCache _cache;
        private const string CACHE_KEY = "all_skills_lookup";

        public SkillService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SkillService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<LookupDto>> GetAllSkillsAsync()
        {
            _logger.LogDebug("Fetching all skills from cache or database");
            var cached = await _cache.GetRecordAsync<List<LookupDto>>(CACHE_KEY);
            if (cached != null)
            {
                _logger.LogInformation("Returning skills from Cache.");
                return cached;
            }

            var skills = await _unitOfWork.Skills.GetAllAsync();
            var dtos = _mapper.Map<List<LookupDto>>(skills);

            await _cache.SetRecordAsync(CACHE_KEY, dtos, TimeSpan.FromHours(24));
            _logger.LogInformation("Retrieved {Count} skill(s) from DATABASE and updated cache", skills.Count);
            return dtos;
        }

        public async Task<LookupDto> GetSkillByIdAsync(int skillId)
        {
            _logger.LogDebug("Fetching skill ID {SkillId}", skillId);
            var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
            if (skill == null)
            {
                _logger.LogWarning("Skill ID {SkillId} not found", skillId);
                throw new NotFoundException("Skill", skillId);
            }
            return _mapper.Map<LookupDto>(skill);
        }

        public async Task<LookupDto> CreateSkillAsync(CreateLookupDto dto)
        {
            _logger.LogInformation("Creating skill: {Name}", dto.Name);
            var existingSkills = await _unitOfWork.Skills.FindAsync(s => s.Name == dto.Name);
            if (existingSkills.Any())
            {
                _logger.LogWarning("Skill creation failed: Name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Skill '{dto.Name}' already exists");
            }

            var skill = _mapper.Map<Skill>(dto);
            skill.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Skills.AddAsync(skill);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Skill created: {Name}. Cache invalidated.", skill.Name);
            return _mapper.Map<LookupDto>(skill);
        }

        public async Task UpdateSkillAsync(int skillId, CreateLookupDto dto)
        {
            _logger.LogInformation("Updating skill ID {SkillId}", skillId);
            var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
            if (skill == null)
            {
                _logger.LogWarning("Update failed: Skill ID {SkillId} not found", skillId);
                throw new NotFoundException("Skill", skillId);
            }

            var duplicates = await _unitOfWork.Skills.FindAsync(s => s.Name == dto.Name && s.Id != skillId);
            if (duplicates.Any())
            {
                _logger.LogWarning("Update failed: Skill name '{Name}' already exists", dto.Name);
                throw new ConflictException($"Skill '{dto.Name}' already exists");
            }

            string oldName = skill.Name;
            skill.Name = dto.Name;
            skill.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Skills.Update(skill);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogInformation("Skill ID {Id} updated: '{OldName}' -> '{NewName}'. Cache invalidated.", skillId, oldName, skill.Name);
        }

        public async Task DeleteSkillAsync(int skillId)
        {
            _logger.LogInformation("Deleting skill ID {SkillId}", skillId);
            var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
            if (skill == null)
            {
                _logger.LogWarning("Delete failed: Skill ID {SkillId} not found", skillId);
                throw new NotFoundException("Skill", skillId);
            }

            var coursesUsingSkill = await _unitOfWork.Courses.FindAsync(c => c.Skills.Any(s => s.Id == skillId));
            if (coursesUsingSkill.Any())
            {
                _logger.LogWarning("Delete failed: Skill ID {SkillId} is in use", skillId);
                throw new ConflictException($"Cannot delete skill '{skill.Name}' - used by courses");
            }

            _unitOfWork.Skills.Delete(skill);
            await _unitOfWork.SaveChangesAsync();

            await _cache.RemoveAsync(CACHE_KEY);
            _logger.LogWarning("Skill ID {Id} ('{Name}') deleted successfully. Cache invalidated.", skillId, skill.Name);
        }
    }
}