using System;

namespace Elekto
{
    /// <summary>
    /// Comportamento das validações customizadas de Certificados SSL/TLS
    /// </summary>
    [Flags]
    public enum CustomCertificateValidationBehaviour
    {
        /// <summary>
        /// Default do Sistema Operacional
        /// </summary>
        SystemDefault = 0,

        /// <summary>
        /// Aceita qualquer coisa
        /// </summary>
        /// <remarks>
        /// Extremamente perigoso! Somente o cliente pode decidir habilitar essa porcaria
        /// </remarks>
        AcceptAll = 1,

        /// <summary>
        /// Loga detalhes do certificado
        /// </summary>
        Log = 2,

        /// <summary>
        /// Ignora certificados expirados
        /// </summary>
        IgnoreExpired = 4,

        /// <summary>
        /// Verifica se o Thumbprint está numa blacklist
        /// </summary>
        BlackListByThumbprint = 8,

        /// <summary>
        /// Verifica se o Thumbprint está numa whitelist
        /// </summary>
        WhiteListByThumbprint = 16,

        /// <summary>
        /// Loga (também) a cadeia do certificado
        /// </summary>
        DumpChain = 32
    }
}