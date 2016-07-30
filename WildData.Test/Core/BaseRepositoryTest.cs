using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Core;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Test.Core
{
    [TestFixture]
    class BaseRepositoryTest
    {
        [Test]
        public void Ctor()
        {

            Expression<Func<IReaderWrapper, Model>> readerWrapper = reader => new Model { Property1 = reader.GetInt(0), Field18 = reader.GetString(1) };

            TestRepository testRepository = new TestRepository();
        }
    }

    class TestRepository : BaseRepository<Model,int>
    {

    }

    [Storage("ModelTable")]
    class Model : IReadOnlyModel<int>
    {
        public int Property1
        {
            get;
            set;
        }

        [Column("Field2",255)]
        public string Property2
        {
            get;
            set;
        }

        [Column("Field3", 255)]
        public string Property3
        {
            get;
            set;
        }

        [Attributes.Ignore]
        public string Property4
        {
            get;
            set;
        }

        [NotNull]
        public string Property5
        {
            get;
            set;
        }

        public string Property6
        {
            get;
            private set;
        }

        public string Property7
        {
            private get;
            set;
        }

        public string Property8
        {
            get;
        }
        public string Property9
        {
            set { }
        }

        protected string Property10
        {
            get;
            set;
        }

        private string Property11
        {
            get;
            set;
        }

        internal string Property12
        {
            get;
            set;
        }
        public static string Property13
        {
            get;
            set;
        }

        protected string Field14 = string.Empty;

        private string Field15 = string.Empty;

        internal string Field16 = string.Empty;

        public static string Field17 = string.Empty;

        public string Field18 = string.Empty;
    }
}
