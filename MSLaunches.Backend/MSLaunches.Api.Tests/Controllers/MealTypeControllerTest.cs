﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MSLunches.Api.Controllers;
using MSLunches.Api.Models.Response;
using MSLunches.Api.Tests.Controllers.MapperConfig;
using MSLunches.Data.Models;
using MSLunches.Domain.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MSLunches.Api.Tests.Controllers
{
    public class MealTypeControllerTest
    {
        private Mock<IMealTypeService> _mealTypeService;
        private IMapper _mapper;

        public MealTypeControllerTest()
        {
            _mealTypeService = new Mock<IMealTypeService>();
            _mapper = new TestMapperConfiguration().CreateMapper();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsListOfMealType()
        {
            var classUnderTest = new MealTypeController(_mealTypeService.Object, _mapper);
            var listOfMealTypes = new List<MealType>()
            {
                GetSampleMealType(1),
                GetSampleMealType(2)
            };

            _mealTypeService.Setup(s => s.GetAsync()).ReturnsAsync(listOfMealTypes);

            var result = await classUnderTest.GetAll();

            var okresult = Assert.IsType<OkObjectResult>(result);
            var resultList = Assert.IsAssignableFrom<IEnumerable<MealTypeDto>>(okresult.Value);

            foreach (var mealType in resultList)
            {
                Assert.Contains(listOfMealTypes, a => a.Id == mealType.Id);
            }

            _mealTypeService.Verify(a => a.GetAsync(), Times.Once);
        }

        #endregion

        #region Get Tests

        [Fact]
        public async Task Get_ReturnsAMealType()
        {
            var classUnderTest = new MealTypeController(_mealTypeService.Object, _mapper);
            var mealType = GetSampleMealType(1);

            _mealTypeService.Setup(s => s.GetByIdAsync(It.Is<int>(i => i == mealType.Id))).ReturnsAsync(mealType);

            var result = await classUnderTest.Get(mealType.Id);

            var okresult = Assert.IsType<OkObjectResult>(result);
            var resultEntity = Assert.IsType<MealTypeDto>(okresult.Value);

            Assert.Equal(mealType.Id, resultEntity.Id);

            _mealTypeService.Verify(s => s.GetByIdAsync(It.Is<int>(i => i == mealType.Id)), Times.Once);
        }

        #endregion

        #region Private Methods

        private MealType GetSampleMealType(int id = 1)
        {
            return new MealType
            {
                Description = "desc1",
                Id = id,
                IsSelectable = true
            };
        }

        #endregion
    }
}
