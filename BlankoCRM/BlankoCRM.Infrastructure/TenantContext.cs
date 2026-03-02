using System;
using System.Threading;

namespace BlankoCRM.Core
{
    public static class TenantContext
    {
        private static readonly AsyncLocal<Guid> _currentTenantId = new AsyncLocal<Guid>();

        public static Guid CurrentTenantId
        {
            get => _currentTenantId.Value;
            set => _currentTenantId.Value = value;
        }
    }
}
