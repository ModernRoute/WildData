using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Test.Models;

namespace ModernRoute.WildData.Test.Core
{
    class Repository
    {
        public const int RandomId = 109683325;
        public const string RandomStringValue = "QLqsxzXzPu4FaxLmwLlc";

        private ReadWriteRepositoryHelper<Model, int> _RepositoryHelper;

        private RowStorage _Wrapper;

        public Repository()
        {
            _RepositoryHelper = new ReadWriteRepositoryHelper<Model, int>(new SupportEverythingTypeKindInfo());
            _Wrapper = new RowStorage();
        }

        public void StoreModel(Model model)
        {
            _RepositoryHelper.SetParametersFromObjectForStore(_Wrapper, model);
            _Wrapper.AddParamNotNull(_RepositoryHelper.MemberColumnMap[nameof(model.Id)].ParamNameBase, RandomId);
            string value = RandomStringValue;
            _Wrapper.AddParam(_RepositoryHelper.MemberColumnMap[nameof(model.Field18)].ParamNameBase, value, value.Length);
            _RepositoryHelper.UpdateVolatileColumnsOnStore(_Wrapper, model);
        }

        public void UpdateModel(Model model)
        {
            _RepositoryHelper.SetParametersFromObjectForUpdate(_Wrapper, model);
            string value = RandomStringValue;
            _Wrapper.AddParam(_RepositoryHelper.MemberColumnMap[nameof(model.Field18)].ParamNameBase, value, value.Length);
            _RepositoryHelper.UpdateVolatileColumnsOnUpdate(_Wrapper, model);
        }

        public Model GetModel()
        {
            return _RepositoryHelper.ReadSingleObject(_Wrapper);
        }
    }

}
