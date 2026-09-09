using System;
using System.Threading;
using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Infrastructure
{
    /// <summary>
    /// Контекст текущей организации.
    /// Использует AsyncLocal для корректной работы в асинхронных сценариях.
    /// </summary>
    public sealed class OrganizationContext : IOrganizationContext
    {
        private static readonly AsyncLocal<Guid?> _currentOrganizationId = new AsyncLocal<Guid?>();

        /// <summary>
        /// Текущий OrganizationId. Может быть null для неавторизованных запросов.
        /// </summary>
        public static Guid? CurrentOrganizationId
        {
            get => _currentOrganizationId.Value;
            set => _currentOrganizationId.Value = value;
        }

        /// <summary>
        /// Сбрасывает контекст.
        /// </summary>
        /// <summary>
        /// Устанавливает организацию текущего асинхронного потока.
        /// </summary>
        public void SetCurrentOrganizationId(Guid organizationId)
        {
            CurrentOrganizationId = organizationId;
        }

        /// <summary>
        /// Возвращает организацию текущего асинхронного потока.
        /// </summary>
        public Guid? GetCurrentOrganizationId()
        {
            return CurrentOrganizationId;
        }

        /// <summary>
        /// Очищает организацию текущего асинхронного потока.
        /// </summary>
        public void Clear()
        {
            _currentOrganizationId.Value = null;
        }
    }
}


