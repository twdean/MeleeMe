using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTwitter;
using Melee.Me.Infrastructure.Repository;
using Moq;
using Xunit;
using MeleeMeDatabase;

namespace Melee.Me.Test
{
    public class When_creating_a_user
    {
        private Mock<ITwitterExecute> execMock;

        TwitterContext InitTwitterContextWithPostToTwitter<TEntity>(string response)
        {
            var authMock = new Mock<ITwitterAuthorizer>();
            execMock = new Mock<ITwitterExecute>();
            execMock.SetupGet(exec => exec.AuthorizedClient).Returns(authMock.Object);
            execMock.Setup(
                exec => exec.PostToTwitter(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<Func<string, TEntity>>()))
                    .Returns(response);

            var ctx = new TwitterContext(execMock.Object);
            return ctx;
        }


        [Fact]
        public void the_repository_save_should_be_called()
        {
            //Arrange
            var mockRepository = new Mock<IUserRepository>();

            //Act

            //Assert

        }
    }
}
