namespace IoCBuilder
{
    /// <summary>
    /// Enumeration to represent the object builder stages.
    /// </summary>
    /// <remarks>
    /// The order of the values in the enumeration is the order in which the stages are run.
    /// </remarks>
    public enum BuilderStage
    {
        /// <summary>
        /// First stage.
        /// By default, nothing happens here.
        /// </summary>
        Setup,

        /// <summary>
        /// Second stage.
        /// Type mapping occurs here.
        /// </summary>
        TypeMapping,

        /// <summary>
        /// Third stage.
        /// Lifetime managers are checked here,
        /// and if they're available the rest of the pipeline is skipped.
        /// </summary>
        Lifetime,

        /// <summary>
        /// Fourth stage.
        /// Reflection on types before interception.
        /// </summary>
        Reflection,

        /// <summary>
        /// Fifth stage.
        /// Intercepting types.
        /// </summary>
        Interception,

        /// <summary>
        /// Sixth stage.
        /// Strategies in this stage run before creation. Typical work done in this stage might
        /// include strategies that use reflection to set policies into the context that other
        /// strategies would later use.
        /// </summary>
        PreCreation,

        /// <summary>
        /// Seventh stage.
        /// Strategies in this stage create objects. Typically you will only have a single policy-driven
        /// creation strategy in this stage.
        /// </summary>
        Creation,

        /// <summary>
        /// Eighth stage.
        /// Strategies in this stage work on created objects. Typical work done in this stage might
        /// include setter injection and method calls.
        /// </summary>
        Initialization,

        /// <summary>
        /// Ninth and final stage.
        /// Strategies in this stage work on objects that are already initialized. Typical work done in
        /// this stage might include looking to see if the object implements some notification interface
        /// to discover when its initialization stage has been completed.
        /// </summary>
        PostInitialization,
    }
}