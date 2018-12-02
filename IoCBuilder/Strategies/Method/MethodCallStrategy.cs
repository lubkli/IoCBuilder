using System.Collections.Generic;
using IoCBuilder.Strategies.Parameters;

namespace IoCBuilder.Strategies.Method
{
    /// <summary>
    /// Represents a strategy to call method.
    /// </summary>
    public class MethodCallStrategy : BuilderStrategy
    {
        /// <summary>
        /// Called during the chain of responsibility for a build operation. Looks for a method call policy for the buildKey and uses it to call a method if found.
        /// </summary>
        /// <param name="context">The context for the operation.</param>
        /// <param name="buildKey">The key for the operation.</param>
        /// <param name="existing">The instance for the operation.</param>
        public override object BuildUp(IBuilderContext context,
                                       object buildKey,
                                       object existing)
        {
            IMethodCallPolicy policy = context.Policies.Get<IMethodCallPolicy>(buildKey);

            if (existing != null)
            {
                if (policy == null)
                {
                    string id = null;
                    BuilderStrategy.TryGetNameFromBuildKey(buildKey, out id);
                    BuildKey newKey = new BuildKey(existing.GetType(), id);

                    policy = context.Policies.Get<IMethodCallPolicy>(newKey);
                }

                if (policy != null)
                {
                    foreach (IMethodCallInfo method in policy.Methods)
                    {
                        if (TraceEnabled(context))
                        {
                            List<object> parameters = new List<object>();

                            foreach (IParameter p in method.Parameters)
                            {
                                parameters.Add(p.GetValue(context));
                            }

                            TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("CallingMethod"), method.MethodName, parameters);
                        }
                        method?.Execute(context, existing, buildKey);
                    }
                }
            }

            return base.BuildUp(context, buildKey, existing);
        }
    }
}