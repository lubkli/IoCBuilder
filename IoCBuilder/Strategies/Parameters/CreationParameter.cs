using System;

namespace IoCBuilder.Strategies.Parameters
{
    /// <summary>
    /// Implementation of <see cref="IParameter"/> which runs the entire build chain to
    /// generate the parameter value.
    /// </summary>
    public class CreationParameter : KnownTypeParameter
    {
        private string creationID;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreationParameter"/> class using
        /// the provided type.
        /// </summary>
        public CreationParameter(Type type)
            : this(type, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreationParameter"/> class using
        /// the provided type and ID.
        /// </summary>
        /// <param name="type">The type of the object to be created.</param>
        /// <param name="id">The ID of the object to be created.</param>
        public CreationParameter(Type type, string id)
            : base(type)
        {
            creationID = id;
        }

        /// <summary>
        /// See <see cref="IParameter.GetValue"/> for more information.
        /// </summary>
        public override object GetValue(IBuilderContext context)
        {
            return context.HeadOfChain.BuildUp(context, new BuildKey(type, creationID), null);
        }
    }
}