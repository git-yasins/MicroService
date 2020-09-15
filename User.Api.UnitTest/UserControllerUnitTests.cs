using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using User.API.Controllers;
using User.API.Data;
using User.API.Models;
using Xunit;

namespace User.API.UnitTest
{
    public class UserControllerUnitTests {

        /// <summary>
        /// 获取EF用户信息上下文
        /// </summary>
        /// <returns></returns>
        private Data.UserContext GetUserContext () {
            var options = new DbContextOptionsBuilder<UserContext> ()
                .UseInMemoryDatabase (Guid.NewGuid ().ToString ()).Options;
            var userContext = new Data.UserContext (options);
            userContext.Users.Add (new Models.AppUser {
                Id = 1,
                    Name = "yasin00"
            });

            userContext.SaveChanges ();
            return userContext;
        }

        /// <summary>
        /// 获取User控制器
        /// </summary>
        /// <returns>返回元组</returns>
        private (UserController controller, UserContext userContext) GetUserController () {
            var context = GetUserContext ();
            var loggerMoq = new Mock<ILogger<UserController>> ();
            var logger = loggerMoq.Object;
            return (controller: new UserController (context, logger,null), userContext : context);
        }

        [Fact]
        public async Task Get_ReturnRightUser_WithExpectedParameters () {
            (UserController controller, UserContext userContext) = GetUserController ();
            var actionResult = await controller.Get ();

            var subject = actionResult.Should ().BeOfType<OkObjectResult> ().Subject;
            var appUser = subject.Value.Should ().BeAssignableTo<AppUser> ().Subject;
            appUser.Id.Should ().Be (1);
            appUser.Name.Should ().Be ("yasin00");
        }

        /// <summary>
        ///  修改名称
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Patch_ReturnNewProperties_WithNewName () {
            (UserController controller, UserContext userContext) = GetUserController ();
            var document = new JsonPatchDocument<AppUser> ();
            //模拟替换用户名称
            document.Replace (user => user.Name, "yasin");
            //调用Action
            var response = await controller.Path (document);

            var result = response.Should ().BeOfType<OkObjectResult> ().Subject;
            //获取执行结果
            var appUser = result.Value.Should ().BeAssignableTo<AppUser> ().Subject;
            appUser.Name.Should ().Be ("yasin");
            //验证数据库修改结果
            var userModel = await userContext.Users.SingleOrDefaultAsync (u => u.Id == 1);
            userModel.Should ().NotBeNull ();
            userModel.Name.Should ().Be ("yasin");
        }

        /// <summary>
        /// 新增用户属性项
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Patch_ReturnNewProperties_WithAddNewProperties () {
            (UserController controller, UserContext userContext) = GetUserController ();
            var document = new JsonPatchDocument<AppUser> ();
            //模拟替换用户名称
            document.Replace (u => u.Properties, new List<UserProperty> {
                new UserProperty {
                    Key = "fin_industry", Value = "互联网", Text = "互联网"
                }
            });
            //调用Action
            var response = await controller.Path (document);

            var result = response.Should ().BeOfType<OkObjectResult> ().Subject;
            //获取执行结果
            var appUser = result.Value.Should ().BeAssignableTo<AppUser> ().Subject;
            appUser.Properties.Count.Should ().Be (1);
            appUser.Properties.First ().Value.Should ().Be ("互联网");
            appUser.Properties.First ().Key.Should ().Be ("fin_industry");
            //验证数据库修改结果
            var userModel = await userContext.Users.SingleOrDefaultAsync (u => u.Id == 1);
            userModel.Properties.Count.Should ().Be (1);
            userModel.Properties.First ().Value.Should ().Be ("互联网");
            userModel.Properties.First ().Key.Should ().Be ("fin_industry");
        }

        /// <summary>
        /// 移除用户属性项
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Patch_ReturnNewProperties_WithRemoveNewProperties () {
           (UserController controller, UserContext userContext) = GetUserController ();
            var document = new JsonPatchDocument<AppUser> ();
            //模拟替换用户名称
            document.Replace (u => u.Properties, new List<UserProperty> {  });
            //调用Action
            var response = await controller.Path (document);
            var result = response.Should ().BeOfType<OkObjectResult> ().Subject;
            //获取执行结果
            var appUser = result.Value.Should ().BeAssignableTo<AppUser> ().Subject;
            appUser.Properties.Should().BeEmpty();
            //验证数据库修改结果
            var userModel = await userContext.Users.SingleOrDefaultAsync (u => u.Id == 1);
            userModel.Properties.Should().BeEmpty();
        }
    }
}