namespace Sitecore.Support.MVC.Pipelines.Response.RenderRendering
{
    using Sitecore.Mvc.Pipelines.Response.RenderRendering;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Mvc.Extensions;
    using Sitecore.Mvc.Presentation;
    /// <summary>The <see cref="GenerateCacheKey"/> class.</summary>
    public class GenerateCacheKey : RenderRenderingProcessor
    {
        #region Public virtual methods

        /// <summary>Processes the specified args.</summary>
        /// <param name="args">The args.</param>
        public override void Process([NotNull] RenderRenderingArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (args.Rendered)
            {
                return;
            }

            if (!args.Cacheable || !args.CacheKey.IsEmptyOrNull())
            {
                return;
            }

            args.CacheKey = this.GenerateKey(args.Rendering, args);
        }

        #endregion

        #region Protected virtual methods

        /// <summary>Generates the key.</summary>
        /// <param name="rendering">The rendering.</param>
        /// <param name="args">The args.</param>
        /// <returns>The result.</returns>
        [CanBeNull]
        protected virtual string GenerateKey([NotNull] Rendering rendering, [NotNull] RenderRenderingArgs args)
        {
            Debug.ArgumentNotNull(rendering, "rendering");
            Debug.ArgumentNotNull(args, "args");

            var cacheKey = rendering.Caching.CacheKey.OrIfEmpty(args.Rendering.Renderer.ValueOrDefault(renderer => renderer.CacheKey));

            if (cacheKey.IsEmptyOrNull())
            {
                return null;
            }

            var key = cacheKey + "_#lang:" + Language.Current.Name.ToUpper()
              + this.GetAreaPart(args);

            var specification = rendering.Caching;

            if (specification.VaryByData)
            {
                key += this.GetDataPart(rendering);
            }

            if (specification.VaryByDevice)
            {
                key += this.GetDevicePart(rendering);
            }

            if (specification.VaryByLogin)
            {
                key += this.GetLoginPart(rendering);
            }

            if (specification.VaryByUser)
            {
                key += this.GetUserPart(rendering);
            }

            if (specification.VaryByParameters)
            {
                key += this.GetParametersPart(rendering);
            }

            if (specification.VaryByQueryString)
            {
                key += this.GetQueryStringPart(rendering);
            }

            return key;
        }

        /// <summary>Gets the area part of the cahcekey.</summary>
        /// <param name="args">RenderRendering pipeline args</param>
        /// <returns>Empty string if no "area" key found in RouteData.DataTokens.
        /// Otherwise a partial cache key indicating the area</returns>
        [NotNull]
        protected virtual string GetAreaPart([NotNull] RenderRenderingArgs args)
        {
            if (args.PageContext.RequestContext.RouteData.DataTokens.ContainsKey("area"))
            {
                return "_#area:" + args.PageContext.RequestContext.RouteData.DataTokens["area"];
            }
            return string.Empty;
        }

        /// <summary>Gets the data part.</summary>
        /// <param name="rendering">The renderer.</param>
        /// <returns>The result.</returns>
        [NotNull]
        protected virtual string GetDataPart([NotNull] Rendering rendering)
        {
            Debug.ArgumentNotNull(rendering, "rendering");

            var item = rendering.Item;

            if (item == null)
            {
                return string.Empty;
            }

            return "_#data:" + item.Paths.Path;
        }

        /// <summary>Gets the device part.</summary>
        /// <param name="rendering">The renderer.</param>
        /// <returns>The result.</returns>
        [NotNull]
        protected virtual string GetDevicePart([NotNull] Rendering rendering)
        {
            Debug.ArgumentNotNull(rendering, "rendering");

            return "_#dev:" + Context.GetDeviceName();
        }

        /// <summary>Gets the login part.</summary>
        /// <param name="rendering">The renderer.</param>
        /// <returns>The result.</returns>
        [NotNull]
        protected virtual string GetLoginPart([NotNull] Rendering rendering)
        {
            Debug.ArgumentNotNull(rendering, "rendering");

            return "_#login:" + Context.IsLoggedIn;
        }

        /// <summary>Gets the parameters part.</summary>
        /// <param name="rendering">The renderer.</param>
        /// <returns>The result.</returns>
        [NotNull]
        protected virtual string GetParametersPart([NotNull] Rendering rendering)
        {
            Debug.ArgumentNotNull(rendering, "rendering");

            return "_#parm:" + rendering.Parameters.ToQueryString();
        }

        /// <summary>Gets the query string part.</summary>
        /// <param name="rendering">The renderer.</param>
        /// <returns>The result.</returns>
        [NotNull]
        protected virtual string GetQueryStringPart([NotNull] Rendering rendering)
        {
            Debug.ArgumentNotNull(rendering, "rendering");

            var site = Context.Site;

            if (site == null)
            {
                return string.Empty;
            }

            var request = site.Request;

            if (request == null)
            {
                return string.Empty;
            }

            return "_#qs:" + MainUtil.ConvertToString(request.QueryString, "=", "&");
        }

        /// <summary>Gets the user part.</summary>
        /// <param name="rendering">The renderer.</param>
        /// <returns>The result.</returns>
        [NotNull]
        protected virtual string GetUserPart([NotNull] Rendering rendering)
        {
            Debug.ArgumentNotNull(rendering, "rendering");

            return "_#user:" + Context.GetUserName();
        }

        #endregion
    }
}