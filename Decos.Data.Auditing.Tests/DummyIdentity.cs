using System;

namespace Decos.Data.Auditing.Tests
{
    internal class DummyIdentity : IIdentity
    {
        public string Id => "5d3a92a3-0849-4617-9e28-484b1f642596";

        public string Name => "Test user";
    }
}