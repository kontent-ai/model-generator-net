using System;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Configuration;

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
                throw new Exception($"You have to provide at least the '{nameof(DeliveryOptions.ProjectId)}' argument. See http://bit.ly/k-params for more details on configuration.");
            }
            else
            {
                DeliveryOptionsValidator.Validate(deliveryOptions);
            }
        }
    }
}
