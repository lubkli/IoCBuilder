using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace IoCBuilder.Location
{
    [TestFixture]
    public class ReadOnlyLocatorFixture
    {
        [Test]
        public void NullInnerLocatorThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ReadOnlyLocator locator = new ReadOnlyLocator(null);
            });
        }

        [Test]
        public void CannotCastAReadOnlyLocatorToAReadWriteLocator()
        {
            Locator innerLocator = new Locator();
            ReadOnlyLocator locator = new ReadOnlyLocator(innerLocator);

            Assert.IsTrue(locator.ReadOnly);
            Assert.IsNull(locator as IReadWriteLocator);
        }

        [Test]
        public void ReadOnlyLocatorCountReflectsInnerLocatorCount()
        {
            Locator innerLocator = new Locator();
            ReadOnlyLocator locator = new ReadOnlyLocator(innerLocator);

            innerLocator.Add(1, 1);
            innerLocator.Add(2, 2);

            Assert.AreEqual(innerLocator.Count, locator.Count);
        }

        [Test]
        public void ParentLocatorOfReadOnlyLocatorIsAlsoReadOnly()
        {
            Locator parentInnerLocator = new Locator();
            Locator childInnerLocator = new Locator(parentInnerLocator);
            ReadOnlyLocator childLocator = new ReadOnlyLocator(childInnerLocator);

            IReadableLocator parentLocator = childLocator.ParentLocator;

            Assert.IsTrue(parentLocator.ReadOnly);
            Assert.IsNull(parentLocator as IReadWriteLocator);
        }

        [Test]
        public void ItemsContainedInLocatorContainedInReadOnlyLocator()
        {
            Locator innerLocator = new Locator();
            ReadOnlyLocator locator = new ReadOnlyLocator(innerLocator);

            innerLocator.Add(1, 1);
            innerLocator.Add(2, 2);

            Assert.IsTrue(locator.Contains(1));
            Assert.IsTrue(locator.Contains(2));
            Assert.IsFalse(locator.Contains(3));
        }

        [Test]
        public void CanEnumerateItemsInReadOnlyLocator()
        {
            Locator innerLocator = new Locator();
            ReadOnlyLocator locator = new ReadOnlyLocator(innerLocator);

            innerLocator.Add(1, 1);
            innerLocator.Add(2, 2);

            bool sawOne = false;
            bool sawTwo = false;

            foreach (KeyValuePair<object, object> pair in locator)
            {
                if (pair.Key.Equals(1))
                    sawOne = true;
                if (pair.Key.Equals(2))
                    sawTwo = true;
            }

            Assert.IsTrue(sawOne);
            Assert.IsTrue(sawTwo);
        }

        [Test]
        public void GenericGetEnforcesDataType()
        {
            Locator innerLocator = new Locator();
            ReadOnlyLocator locator = new ReadOnlyLocator(innerLocator);

            innerLocator.Add(1, 2);

            Assert.Throws<InvalidCastException>(() => locator.Get<string>(1));
        }

        [Test]
        public void GenericGetWithSearchModeEnforcesDataType()
        {
            Locator innerLocator = new Locator();
            ReadOnlyLocator locator = new ReadOnlyLocator(innerLocator);

            innerLocator.Add(1, 2);

            Assert.Throws<InvalidCastException>(() => locator.Get<string>(1, SearchMode.Local));
        }
    }
}