﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TennisClub.AppCore.Model.impl;
using TennisClub.AppCore.Validators;
using TennisClub.Data.Model;
using TennisClub.Data.Repository.interfaces;
using TennisClub.Infrastructure.Interfaces;
using TennisClub.Infrastructure.Mappers;

namespace TennisClub.Infrastructure.Services
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IChildService childService;
        private readonly IMapper<Group, GroupInDb> groupMapperToDb;
        private readonly IMapper<GroupInDb, Group> groupMapperFromDb;
        private readonly ChildAgeRuleChecker ageRuleChecker;
        private readonly int maxChildren;
        
        public GroupService(IServiceProvider serviceProvider)
        {
            this.unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            this.childService = serviceProvider.GetRequiredService<IChildService>();
            
            this.groupMapperToDb = new GroupToGroupInDbMapper();
            this.groupMapperFromDb = new GroupInDbNullableToGroupMapper();
            
            this.ageRuleChecker = new ChildAgeRuleChecker();
            this.maxChildren = Convert.ToInt32(
                ConfigurationManager.AppSettings["maxAmountOfChildrenInGroup"] ?? "5");
        }

        public List<Group> TryAddChildToGroup(Child child)
        {
            List<Group> groups = FindVaccantGroups(child);
            if (groups.Count <= 1)
            {
                Group group = groups.FirstOrDefault() 
                              ?? CreateGroup(child.GameLevel, child.LessonsDay);
                
                if (groups.Count == 0) ChooseDay(child, group);
                unitOfWork.SaveChanges();
                AddChildToGroup(child, group);
                groups = new List<Group> {group};
            }
            return groups;
        }

        public void AddChildToGroup(Child child, Group group)
        {
            child.LessonsDay = group.LessonsDay;
            childService.SetChildToGroup(child, group);
            unitOfWork.SaveChanges();
        }

        private List<Group> FindVaccantGroups(Child child)
        {
            List<Group> groups = unitOfWork.GroupRepository
                .FindVacantGroups(
                    child.PreferableDays,
                    child.GameLevel,
                    maxChildren,
                    ageRuleChecker.CreateRuleCheckerDelegate(child))
                .Select(groupMapperFromDb.Map)
                .ToList();
            return groups;
        }

        private Group CreateGroup(GameLevel gameLevel, DayOfWeek dayOfWeek)
        {
            Group group = new Group(gameLevel, dayOfWeek);
            return group;
        }

        private void ChooseDay(Child child, Group group)
        {
            var rand = new Random();
            var index = rand.Next(child.PreferableDays.Count);
            child.LessonsDay = child.PreferableDays[index];
            group.LessonsDay = child.LessonsDay;
            
            unitOfWork.GroupRepository.Create(
                groupMapperToDb.Map(group));
        }
    }
}
