using Kentico.Kontent.Delivery;
using System;

namespace Kentico.Kontent.ModelGenerator
{
    /// <summary>
    /// Extension methods meant for validation.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validates that DeliveryOptions are initialized and performs some extra integrity validations.
        /// </summary>
        /// <param name="deliveryOptions">DeliveryOptions object to be validated</param>
        public static void Validate(this DeliveryOptions deliveryOptions)
        {
            if (deliveryOptions == null)
            {
                throw new Exception("You have to define 'DeliveryOptions' section in your config file or at least provide a Project ID.");
            }
            else
            {
                DeliveryOptionsValidator.Validate(deliveryOptions);
            }
        }
    }
}
