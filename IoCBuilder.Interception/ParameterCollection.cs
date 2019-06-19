using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace IoCBuilder.Interception
{
    /// <summary>
    /// Exposes an enumerator, which supports a simple iteration over collection of parameters.
    /// </summary>
    public class ParameterCollection : IParameterCollection
    {
        private readonly List<ArgumentInfo> argumentInfo;
        private readonly object[] arguments;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="argumentInfo"></param>
        public ParameterCollection(object[] arguments,
                                   ParameterInfo[] argumentInfo)
            : this(arguments, argumentInfo, delegate
                                            {
                                                return true;
                                            })
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="argumentInfo"></param>
        /// <param name="isArgumentPartOfCollection"></param>
        protected ParameterCollection(object[] arguments,
                                      ParameterInfo[] argumentInfo,
                                      Predicate<ParameterInfo> isArgumentPartOfCollection)
        {
            this.arguments = arguments;
            this.argumentInfo = new List<ArgumentInfo>();

            for (int idx = 0; idx < argumentInfo.Length; ++idx)
                if (isArgumentPartOfCollection(argumentInfo[idx]))
                    this.argumentInfo.Add(new ArgumentInfo(idx, argumentInfo[idx]));
        }

        /// <summary>
        /// Indexer returning parametr for specified position.
        /// </summary>
        /// <param name="index">Parameter's position.</param>
        /// <returns>The parameter.</returns>
        public object this[int index]
        {
            get { return arguments[argumentInfo[index].Index]; }
            set { arguments[argumentInfo[index].Index] = value; }
        }

        /// <summary>
        /// Indexer returning parametr for specified name.
        /// </summary>
        /// <param name="paramName">Parameter's name.</param>
        /// <returns>The parameter.</returns>
        public object this[string paramName]
        {
            get { return arguments[IndexForParameterName(paramName)]; }
            set { arguments[IndexForParameterName(paramName)] = value; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns> An System.Collections.IEnumerator object that can be used to iterate through
        /// the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < argumentInfo.Count; ++i)
                yield return arguments[argumentInfo[i].Index];
        }

        private int IndexForParameterName(string paramName)
        {
            for (int i = 0; i < argumentInfo.Count; ++i)
                if (argumentInfo[i].Name == paramName)
                    return argumentInfo[i].Index;

            throw new ArgumentException("Invalid parameter Name", "paramName");
        }

        private struct ArgumentInfo
        {
            public readonly int Index;
            public readonly string Name;

            public ArgumentInfo(int index,
                                ParameterInfo parameterInfo)
            {
                Index = index;
                Name = parameterInfo.Name;
            }
        }
    }
}