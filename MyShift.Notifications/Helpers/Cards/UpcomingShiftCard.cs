using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Notifications.Helpers.Cards
{
    public static class UpcomingShiftCard
    {
        public static Attachment GetCard()
        {

            string cardTitle = $"Card";
            string cardDescription = $"Hello! , Please sign in for today.";

            

            List<AdaptiveFact> facts = new List<AdaptiveFact>() {
                new AdaptiveFact { Title = "Event: " },
             
            };

           
            AdaptiveCard upcomingShiftCard = new AdaptiveCard("1.2")
            {
                Body = new List<AdaptiveElement>
                 {
                    new AdaptiveTextBlock
                    {
                        Wrap = true,
                        Text = cardTitle,
                        Size = AdaptiveTextSize.ExtraLarge,
                        Weight = AdaptiveTextWeight.Bolder
                    },
                    new AdaptiveTextBlock
                    {
                        Wrap = true,
                        Text = cardDescription,
                    },

                    new AdaptiveFactSet
                    {

                        Facts = facts
                    },
                 },

            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = upcomingShiftCard,
            };
        }


    }
}
