using CityInfo.API.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CityInfo.API
{
    public static class CityInfoContextExtensions
    {
        public static void EnsureSeedDataForContext(this CityInfoContext context)
        {
            if (context.Cities.Any())
            {
                return;
            }

            // init seed data...
            var cities = new List<City>()
            {
                new City()
                {
                    Name = "New York City",
                    Description = "The one with that big park.",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "Central Park",
                            Description = "The most visited urban park in US."
                        },
                        new PointOfInterest()
                        {
                            Name = "Empire State Building",
                            Description = "A 102-story skyscraper."
                        }
                    }
                },
                new City()
                {
                    Name = "Antwerp",
                    Description = "The one with the cathedral that was never really finished.",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "Cathedral",
                            Description = "Not finished."
                        },
                        new PointOfInterest()
                        {
                            Name = "Something",
                            Description = "Bla bla bla."
                        }
                    }
                },
                new City()
                {
                    Name = "Paris",
                    Description = "The one with that big tower.",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "Eiffel Tower",
                            Description = "Famous tower."
                        },
                        new PointOfInterest()
                        {
                            Name = "Something",
                            Description = "Bla bla bla."
                        }
                    }
                }
            };

            context.Cities.AddRange(cities);
            context.SaveChanges();
        }
    }
}
