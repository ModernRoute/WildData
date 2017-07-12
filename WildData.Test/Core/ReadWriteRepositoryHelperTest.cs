using ModernRoute.WildData.Test.Models;
using NUnit.Framework;

namespace ModernRoute.WildData.Test.Core
{
    [TestFixture]
    public class ReadWriteRepositoryHelperTest
    {
        [Test]
        public void Ctor()
        {
            Repository testRepository = new Repository();

            Model model = new Model
            {
                Id = 5,
                Property1 = 54,
                Property2 = "value1",
                Property3 = "value2",
                Property4 = "value3",
                Property5 = "value4",
                Property7 = "value5",
                Property9 = "value7",
                Property12 = "value8",
                Field16 = "value9",
                Field18 = "value10"
            };

            Model expectedStoredModel = model.Clone();
            expectedStoredModel.Id = Repository.RandomId;
            expectedStoredModel.Field18 = Repository.RandomStringValue;

            Model expectedUpdatedModel = model.Clone();
            expectedUpdatedModel.Field18 = Repository.RandomStringValue;

            Model modelToStore = model.Clone();
            testRepository.StoreModel(modelToStore);
            Assert.AreEqual(expectedStoredModel, modelToStore);
            Model actualModel = testRepository.GetModel();
            Assert.AreEqual(expectedStoredModel, actualModel);

            Model modelToUpdate = model.Clone();
            testRepository.UpdateModel(modelToUpdate);
            Assert.AreEqual(expectedUpdatedModel, modelToUpdate);
            actualModel = testRepository.GetModel();
            Assert.AreEqual(expectedUpdatedModel, actualModel);
        }
    }
}
