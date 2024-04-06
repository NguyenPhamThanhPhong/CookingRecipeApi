using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CookingRecipeApi.Helper
{
    public class TimeSpanFormBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;// "cookTime" field name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if ( UInt32.TryParse(value,out uint seconds) )
            {
                bindingContext.Result = ModelBindingResult.Success(TimeSpan.FromSeconds(seconds));
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                                       modelName, "Invalid timespan format");
            }

            return Task.CompletedTask;
        }
    }
    public class TimeSpanFormBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(TimeSpan))
            {
                return new TimeSpanFormBinder();
            }

            return null;
        }
    }
}
