using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class GeneralConfiguration : IGeneralConfiguration
    {
        #region Implementation of IGeneralConfiguration

        public bool UseEvaluationCache { get; set; }

        #endregion
    }
}