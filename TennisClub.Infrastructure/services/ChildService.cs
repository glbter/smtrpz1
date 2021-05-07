﻿using System;
using System.Linq;
using TennisClub.AppCore.Model.impl;
using TennisClub.Data.Model;
using TennisClub.Data.Repository.interfaces;
using TennisClub.Infrastructure.Interfaces;
using TennisClub.Infrastructure.Mappers;

namespace TennisClub.Infrastructure.Services
{
    public class ChildService : IChildService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper<Child, ChildInDb> childMapperToDb;
        private readonly IMapper<Group, GroupInDb> groupMapperToDb;
        public ChildService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            this.childMapperToDb = new ChildToChildInDbMapper();
            this.groupMapperToDb = new GroupToGroupInDbMapper();
        }

        public void SetChildToGroup(Child child, Group group)
        {
            child.GroupId = group.Id;
            AddChild(child);
            IncrementGroup(group);
        }

        public void AddChild(Child child)
        {
            unitOfWork.ChildRepository.Create(
                childMapperToDb.Map(child));
            var days = child.PreferableDays
                .Select(it => ToChildDay(child, it))
                .ToList();
            unitOfWork.ChildChosenDaysRepository
                .BulkInsert(days);
        }

        private void IncrementGroup(Group group)
        {
            var grp = unitOfWork.GroupRepository.FindById(group.Id);
            grp.ChildrenAmount += 1;
            unitOfWork.GroupRepository.Update(grp);
        }
        
        private ChildChosenDaysEntity ToChildDay(Child child, DayOfWeek day)
        {
            return new ChildChosenDaysEntity(child.Id, day);
        }
    }
}
