using AutoMapper;
using BankingApi.Controllers;
using BankingApi.DataAccess;
using BankingApi.Dto;
using BankingApi.MapProfiles;
using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BankingApi
{
    public class InstitutionControllerTests
    {
        private readonly InstitutionController _controller;
        private readonly Mock<IInstitutionRepository> _mockInstitutionRepository;

        public InstitutionControllerTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DtoToModelProfile());
                cfg.AddProfile(new ModelToDtoProfile());
            });
            var mapper = mockMapper.CreateMapper();

            _mockInstitutionRepository = new Mock<IInstitutionRepository>();
            _controller = new InstitutionController(_mockInstitutionRepository.Object, mapper);
        }

        [Fact]
        public async Task Get_ReturnsInstitutionList()
        {
            //Setup institutions returned by mock data access call
            List<Institution> institutions = new List<Institution>
            {
                new Institution { InstitutionId = 1, Name = "First Institution"},
                new Institution { InstitutionId = 2, Name = "Second Institution"},
            };

            _mockInstitutionRepository.Setup(x => x.GetInstitutions(null)).
                Returns(Task.FromResult(institutions));

            //Act
            var actionResult = await _controller.Get() as ObjectResult;
            List<InstitutionDto> result = (List<InstitutionDto>)actionResult.Value;

            //Assert
            Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].InstitutionId);
            Assert.Equal("First Institution", result[0].Name);
            Assert.Equal(2, result[1].InstitutionId);
            Assert.Equal("Second Institution", result[1].Name);
        }

        [Fact]
        public async Task Get_ReturnsEmptyList_WhenNoInstitutionsExists()
        {
            //Setup to return empty institutions from mock data access call
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(null)).
               Returns(Task.FromResult(new List<Institution>()));

            //Act
            var actionResult = await _controller.Get() as ObjectResult;
            List<InstitutionDto> result = (List<InstitutionDto>)actionResult.Value;

            //Assert
            Assert.IsType<OkObjectResult>(actionResult);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetById_ReturnsInstitution_WhenInstitutionExist()
        {
            //Setup to return one institution by mock data access call
            List<Institution> institutions = new List<Institution>
            {
                new Institution { InstitutionId = 1, Name = "First Institution"},
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(1)).
                Returns(Task.FromResult(institutions));

            //Act
            var actionResult = await _controller.GetById(1) as ObjectResult;
            InstitutionDto result = (InstitutionDto)actionResult.Value;

            //Assert
            Assert.IsType<OkObjectResult>(actionResult);
            Assert.NotNull(result);
            Assert.Equal(1, result.InstitutionId);
            Assert.Equal("First Institution", result.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenInstitutionDoesNotExist()
        {
            //Setup to return empty institution list
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(100)).
                Returns(Task.FromResult(new List<Institution>()));

            //Act
            var actionResult = await _controller.GetById(100) as NotFoundResult;

            //Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task Post_CreatesInstitution_WhenRequestIsValid()
        {
            //Setup request object and to return empty institution list from GetInstitutions call
            var request = new InstitutionDto
            {
                InstitutionId = 1,
                Name = "First Institution"
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(1)).
               Returns(Task.FromResult(new List<Institution>()));

            //Act
            var actionResult = await _controller.Post(request) as CreatedAtActionResult;

            //Assert
            _mockInstitutionRepository.Verify(x => x.AddInstitution(
                It.Is<Institution>(x => x.InstitutionId == request.InstitutionId
                && x.Name == request.Name)), Times.Once);
            Assert.IsType<CreatedAtActionResult>(actionResult);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenInstitutionAlreadyExist()
        {
            //Setup to return an institution from GetInstitutionByName mock data access call
            var request = new InstitutionDto
            {
                Name = "First Institution"
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutionByName("First Institution")).
               Returns(Task.FromResult(new List<Institution>() { 
                    new Institution () {InstitutionId = 1, Name = "First Institution"}
               }));

            //Act
            var actionResult = await _controller.Post(request) as BadRequestObjectResult;
            var errorResponse = (SerializableError)actionResult.Value;

            //Assert. Verify AddInstitution is not called
            _mockInstitutionRepository.Verify(x => x.AddInstitution(It.IsAny<Institution>()), Times.Never);
            Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Single(errorResponse);
            Assert.True(errorResponse.ContainsKey("institutionId"));
            Assert.Equal("Institution already exists.", ((string[])errorResponse["institutionId"])[0]);
        }
    }
}
