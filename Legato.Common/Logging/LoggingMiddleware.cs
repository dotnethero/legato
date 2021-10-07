using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Extensions.Logging;

namespace Legato.Common.Logging
{
    class LoggingMiddleware : IResolveMiddleware
    {
        public PipelinePhase Phase => PipelinePhase.ParameterSelection;

        readonly IEnumerable<ResolvedParameter> parameters;

        public LoggingMiddleware()
        {
            parameters = GetParameters();
        }

        public void Execute(ResolveRequestContext context, System.Action<ResolveRequestContext> next)
        {
            context.ChangeParameters(context.Parameters.Union(parameters));
            next(context);
        }

        static IEnumerable<ResolvedParameter> GetParameters()
        {
            yield return new ResolvedParameter(
                (p, i) => p.ParameterType == typeof(ILogger),
                (p, i) => i.Resolve<ILoggerFactory>().CreateLogger(p.Member.DeclaringType));
        }
    }
}
