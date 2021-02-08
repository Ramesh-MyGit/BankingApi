using AutoMapper;
using BankingApi.Controllers;
using BankingApi.DataAccess;
using BankingApi.Dto;
using BankingApi.MapProfiles;
using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BankingApi
{
    public class MemberControllerTests
    {
        private readonly MemberController _controller;
        private readonly Mock<IMemberRepository> _mockMemberRepository;
        private readonly Mock<IInstitutionRepository> _mockInstitutionRepository;

        public MemberControllerTests()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DtoToModelProfile());
                cfg.AddProfile(new ModelToDtoProfile());
            });
            var mapper = mockMapper.CreateMapper();

            _mockInstitutionRepository = new Mock<IInstitutionRepository>();
            _mockMemberRepository = new Mock<IMemberRepository>();
            _controller = new MemberController(_mockInstitutionRepository.Object, 
                _mockMemberRepository.Object, mapper);
        }


        [Fact]
        public async Task Get_ReturnsMembersList()
        {
            //Setup member returned by mock GetMembers call
            var members = SetupMembers(2);
            _mockMemberRepository.Setup(x => x.GetMembers(null)).
                Returns(Task.FromResult(members));

            //Act
            var actionResult = await _controller.Get() as ObjectResult;
            List<MemberDto> result = (List<MemberDto>)actionResult.Value;

            //Assert
            Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].MemberId);
            Assert.Single(result[0].Accounts);

            Assert.Equal(2, result[1].MemberId);
            Assert.Equal(2, result[1].Accounts.Count());

            Assert.Equal("Given Name 1", result[0].GivenName);
            Assert.Equal("Surname 1", result[0].Surname);
            Assert.Equal(1, result[0].InstitutionId);
            Assert.Equal(1, result[0].Accounts.First().AccountId);
            Assert.Equal(10.25m, result[0].Accounts.First().Balance);
        }

        [Fact]
        public async Task GetById_ReturnsMember_WhenMemberExist()
        {
            //Setup to return a member
            var members = SetupMembers(1);
            _mockMemberRepository.Setup(x => x.GetMembers(1)).
                   Returns(Task.FromResult(members));

            //Act
            var actionResult = await _controller.GetById(1) as ObjectResult;
            MemberDto result = (MemberDto)actionResult.Value;

            //Assert
            Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(1, result.MemberId);
            Assert.Single(result.Accounts);
            Assert.Equal("Given Name 1", result.GivenName);
            Assert.Equal("Surname 1", result.Surname);
            Assert.Equal(1, result.InstitutionId);
            Assert.Equal(1, result.Accounts.First().AccountId);
            Assert.Equal(10.25m, result.Accounts.First().Balance);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMemberNotExist()
        {
            //Setup to return empty member list
            _mockMemberRepository.Setup(x => x.GetMembers(1)).
                   Returns(Task.FromResult(new List<Member>()));

            //Act
            var actionResult = await _controller.GetById(1) as NotFoundResult;

            //Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task Post_AddMemberSuccessful_WhenRequestIsValid()
        {
            //Setup to return an institution 
            var members = SetupMembers(1).First();
            var request = new MemberDto
            {
                InstitutionId = 100
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(100)).
                Returns(Task.FromResult(new List<Institution>() { 
                    new Institution {InstitutionId = 1, Name = "First Institution"}
                }));
            _mockMemberRepository.Setup(x => x.AddMember(It.IsAny<Member>())).
                Returns(Task.FromResult(members));

            //Act
            var actionResult = await _controller.Post(request) as CreatedAtActionResult;

            //Assert. Member Id is auto increment in data provider so itwill not be set in the call to data access 
            _mockMemberRepository.Verify(x => x.AddMember(
                It.Is<Member>(x => x.InstitutionId == request.InstitutionId
                && x.MemberId == 0)), Times.Once);
            Assert.IsType<CreatedAtActionResult>(actionResult);
        }

        [Fact]
        public async Task Post_ReturnsNotFound_WhenInstitutionNotFound()
        {
            //Setup to return empty institution list
            var request = new MemberDto
            {
                InstitutionId = 100
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(100)).
                Returns(Task.FromResult(new List<Institution>()));

            //Act
            var actionResult = await _controller.Post(request) as NotFoundObjectResult;
            var errorResponse = (ModelStateDictionary)actionResult.Value;

            //Assert. Verify no add call when validation fails.
            _mockMemberRepository.Verify(x => x.AddMember(
                It.IsAny<Member>()), Times.Never);
            Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("institutionId"));
            Assert.Equal("Institution not found.", errorResponse["institutionId"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Put_UpdateMemberSuccessful_WhenRequestIsValid()
        {
            //Setup to return an institution and member from mock data access call
            var members = SetupMembers(1);
            var request = new MemberDto
            {
                InstitutionId = 100,
                Accounts = new List<AccountDto>() {}
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(100)).
                Returns(Task.FromResult(new List<Institution>() {
                    new Institution {InstitutionId = 1, Name = "First Institution"}
                }));
            _mockMemberRepository.Setup(x => x.GetMembers(1)).
                Returns(Task.FromResult(members));

            //Act
            var actionResult = await _controller.Put(1, request) as NoContentResult;

            //Assert
            _mockMemberRepository.Verify(x => x.UpdateMember(
                It.Is<Member>(x => x.InstitutionId == request.InstitutionId
                && x.MemberId == 1)), Times.Once);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenInstitutionNotFound()
        {
            //Setup to return empty institution list
            var request = new MemberDto
            {
                InstitutionId = 100
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(100)).
                Returns(Task.FromResult(new List<Institution>()));

            //Act
            var actionResult = await _controller.Put(1, request) as NotFoundObjectResult;
            var errorResponse = (ModelStateDictionary)actionResult.Value;

            //Assert. Verify no update call when validation fails.
            _mockMemberRepository.Verify(x => x.UpdateMember(
                It.IsAny<Member>()), Times.Never);
            Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("institutionId"));
            Assert.Equal("Institution not found.", errorResponse["institutionId"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Put_ReturnsNotFound_WhenMemberNotFound()
        {
            //Setup to return empty member list
            var request = new MemberDto
            {
                InstitutionId = 100,
                Accounts = new List<AccountDto>() { }
            };
            _mockInstitutionRepository.Setup(x => x.GetInstitutions(100)).
                Returns(Task.FromResult(new List<Institution>() {
                    new Institution {InstitutionId = 1, Name = "First Institution"}
                }));
            _mockMemberRepository.Setup(x => x.GetMembers(1)).
                Returns(Task.FromResult(new List<Member>()));

            //Act
            var actionResult = await _controller.Put(1, request) as NotFoundObjectResult;
            var errorResponse = (ModelStateDictionary)actionResult.Value;

            //Assert. Verify no update call when validation fails.
            _mockMemberRepository.Verify(x => x.UpdateMember(
                It.IsAny<Member>()), Times.Never);
            Assert.IsType<NotFoundObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("memberId"));
            Assert.Equal("Member not found.", errorResponse["memberId"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMemberNotFound()
        {
            //Seup to return empty member list
            _mockMemberRepository.Setup(x => x.GetMembers(1)).
               Returns(Task.FromResult(new List<Member>()));

            //Act
            var actionResult = await _controller.Delete(1) as NotFoundResult;

            //Assert. Verify no delete call when validation fails.
            _mockMemberRepository.Verify(x => x.DeleteMember(
                It.IsAny<Member>()), Times.Never);
            Assert.IsType<NotFoundResult>(actionResult);

        }

        [Fact]
        public async Task Delete_DeleteMemberSuccessful_WhenRequestIsValid()
        {
            //Setup to return member from mock data access call
            var members = SetupMembers(1);
            _mockMemberRepository.Setup(x => x.GetMembers(1)).
                Returns(Task.FromResult(members));

            //Act
            var actionResult = await _controller.Delete(1) as NoContentResult;

            //Assert
            _mockMemberRepository.Verify(x => x.DeleteMember(
                It.Is<Member>(x => x.InstitutionId == 1
                && x.MemberId == 1)), Times.Once);
            Assert.IsType<NoContentResult>(actionResult);
        }

        private List<Member> SetupMembers(int count)
        {
            List<Member> members = new List<Member>
            {
                new Member {
                    MemberId = 1, GivenName = "Given Name 1", Surname = "Surname 1", InstitutionId  = 1,
                    Accounts = new List<Account> {
                        new Account { AccountId = 1, Balance = 10.25m}
                    }
                }
                
            };

            if (count == 2)
            {
                members.Add(new Member
                {
                    MemberId = 2,
                    GivenName = "Given Name 2",
                    Surname = "Surname 2",
                    InstitutionId = 2,
                    Accounts = new List<Account> {
                        new Account { AccountId = 2, Balance = 6.00m},
                        new Account { AccountId = 3, Balance = 7.00m}
                    }
                });
            }

            return members;
        }
    }
}
