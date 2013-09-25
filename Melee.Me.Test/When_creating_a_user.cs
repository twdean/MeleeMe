using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melee.Me.Persistence;
using Moq;
using Xunit;
using MeleeMeDatabase;

namespace Melee.Me.Test
{
    public class When_creating_a_user
    {
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
