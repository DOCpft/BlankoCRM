using System;
using System.Threading;

namespace BlankoCRM.Infrastructure
{
    /// <summary>
    /// Контекст текущего арендатора (мастера).
    /// Использует AsyncLocal для корректной работы в асинхронных сценариях.
    /// </summary>
    public static class TenantContext
    {
        private static readonly AsyncLocal<Guid?> _currentTenantId = new AsyncLocal<Guid?>();

        /// <summary>
        /// Текущий TenantId. Может быть null для неавторизованных запросов.
        /// </summary>
        public static Guid? CurrentTenantId
        {
            get => _currentTenantId.Value;
            set => _currentTenantId.Value = value;
        }

        /// <summary>
        /// Сбрасывает контекст (для неавторизованных запросов).
        /// </summary>
        public static void Clear()
        {
            _currentTenantId.Value = null;
        }
    }
}

