using FubuMVC.Core;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace FubuMVC.ServerSentEvents.Testing
{
    [TestFixture]
    public class AssemblyNeedsTheFubuModuleAttribute
    {
        [Test]
        public void the_attribute_exists()
        {
            var assembly = typeof (ServerEvent).Assembly;
        
            assembly.GetCustomAttributes(typeof(FubuModuleAttribute), true)
                .Any().ShouldBeTrue();
        }
    }
}