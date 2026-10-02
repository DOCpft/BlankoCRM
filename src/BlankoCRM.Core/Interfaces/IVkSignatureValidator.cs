using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Interfaces
{
    /// <summary>
    /// Проверяет подпись параметров запуска VK Mini App.
    /// </summary>
    public interface IVkSignatureValidator
    {
        /// <summary>
        /// Проверяет подпись <c>sign</c> для переданных параметров запуска.
        /// </summary>
        /// <param name="vkParams">Словарь параметров запуска (включая <c>sign</c>).</param>
        /// <param name="sign">Подпись из параметров запуска.</param>
        /// <returns><c>true</c>, если подпись валидна.</returns>
        bool Validate(IReadOnlyDictionary<string, string> vkParams, string sign);
    }
}
