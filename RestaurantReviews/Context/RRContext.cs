using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SoftWriters.Models;
using System.Net.Http;

namespace RestaurantReviews.Context
{
    #region RRContext
    public class RRContext : DbContext
    {
        public DbSet<Restaurant> Restaurant { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<Reviewer> Reviewer { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<ReviewLocation> ReviewLocation { get; set; }

        public RRContext() : base("DefaultConnection"){}

        #region GetAllRestaurantsAndReviews
        public IQueryable GetAllRestaurantsAndReviews()
        {
            try
            {
                return (from restaurants in this.Restaurant
                        where restaurants.IsDeleted == false
                        select new
                        {
                            Id = restaurants.Id,
                            Name = restaurants.Name,
                            Cities = from address in this.Address
                                     where restaurants.Id == address.EntityId &&
                                           address.IsDeleted == false
                                     select new
                                     {
                                         AddressId = address.Id,
                                         City = (address.City + ", " + address.State)
                                     },
                            Reviews = from reviews in this.Review
                                      where restaurants.Id == reviews.EntityId &&
                                            reviews.IsDeleted == false
                                      select new
                                      {
                                          ReviewId = reviews.Id,
                                          Review = reviews.ReviewText,
                                          Rating = reviews.Rating,
                                          ReviewDate = reviews.CreatedDateTime,
                                          Reviewer = from reviewers in this.Reviewer
                                                     where reviews.ReviewerId == reviewers.Id &&
                                                           reviewers.IsDeleted == false
                                                     select new
                                                     {
                                                         ReviewerId = reviewers.Id,
                                                         UserName = reviewers.UserName
                                                     }
                                      },

                        });

            }
            catch
            {
                throw; 
            }
        }
        #endregion

        #region GetAllRestaurantsByCity
        public IQueryable GetAllRestaurantsByCity(string city)
        {
            try
            {
                return (from address in this.Address
                        join restaurants in this.Restaurant on address.EntityId equals restaurants.Id
                        where address.City.ToLower() == city.Trim().ToLower() &&
                              address.IsDeleted == false && restaurants.IsDeleted == false
                        group new { address, restaurants } by
                        new
                        {
                            RestName = restaurants.Name,
                            RestId = restaurants.Id,
                            RestAddyId = address.Id,
                            RestCity = (address.City + ", " + address.State)
                        } into restGrpByCity
                        select new
                        {
                            Id = restGrpByCity.Key.RestId,
                            Name = restGrpByCity.Key.RestName,
                            AddressId = restGrpByCity.Key.RestAddyId,
                            City = restGrpByCity.Key.RestCity,
                            Reviews = from reviews in this.Review
                                      join reviewloc in this.ReviewLocation on reviews.Id equals reviewloc.ReviewId
                                      where restGrpByCity.Key.RestId == reviews.EntityId &&
                                            reviewloc.IsDeleted == false && reviews.IsDeleted == false &&
                                            reviewloc.AddressId == restGrpByCity.Key.RestAddyId
                                      select new
                                      {
                                          ReviewId = reviews.Id,
                                          Review = reviews.ReviewText,
                                          Rating = reviews.Rating,
                                          ReviewDate = reviews.CreatedDateTime,
                                          Reviewer = from reviewers in this.Reviewer
                                                     where reviews.ReviewerId == reviewers.Id &&
                                                           reviewers.IsDeleted == false
                                                     select new
                                                     {
                                                         ReviewerId = reviewers.Id,
                                                         UserName = reviewers.UserName
                                                     }

                                      },

                        });
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region GetAllRestaurantReviewsByUser
        public IQueryable GetAllRestaurantReviewsByUser(string user)
        {
            try
            {
                return (from reviews in this.Review
                        join reviewers in this.Reviewer on reviews.ReviewerId equals reviewers.Id
                        where reviewers.UserName.ToLower() == user.Trim().ToLower() &&
                              reviewers.IsDeleted == false && reviews.IsDeleted == false
                        select new
                        {
                            ReviewId = reviews.Id,
                            ReviewerId = reviewers.Id,
                            Reviewer = reviewers.UserName,
                            ReviewDate = reviews.CreatedDateTime.Value,
                            RestaurantId = reviews.EntityId,
                            RestaurantAddressId = (from address in this.Address
                                                   join revloc in this.ReviewLocation on address.Id equals revloc.AddressId
                                                   where revloc.ReviewId == reviews.Id && address.IsDeleted == false
                                                   select address.Id),
                            Restaurant = (from restaurant in this.Restaurant where restaurant.Id == reviews.EntityId && restaurant.IsDeleted == false select restaurant.Name),
                            RestaurantCity = (from address in this.Address
                                              join revloc in this.ReviewLocation on address.Id equals revloc.AddressId
                                              where revloc.ReviewId == reviews.Id && address.IsDeleted == false
                                              select (address.City + ", " + address.State)),
                            Review = reviews.ReviewText,
                            Rating = reviews.Rating
                        });
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region AddReview
        public void AddReview(int reviewerId, int restaurantId, int restAddressId, string reviewText, string rating)
        {
            try
            {
                Review rv = new Review
                {
                    ReviewText = reviewText,
                    Rating = int.TryParse(rating, out int outRating) ? int.Parse(rating) : 0,
                    EntityId = restaurantId,
                    ReviewerId = reviewerId
                };

                this.Review.Add(rv);
                this.SaveChanges();

                this.ReviewLocation.Add(new ReviewLocation
                {
                    AddressId = restAddressId,
                    ReviewId = rv.Id
                });

                this.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region DeleteReview
        public void DeleteReview(int reviewId)
        {
            try
            {
                Review review = (from reviews in this.Review where reviews.Id == reviewId select reviews).Single();
                review.IsDeleted = true;
                this.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region AddReviewUser
        public void AddReviewUser(string username, string firstName, string lastName)
        {
            try
            {
                this.Reviewer.Add(new Reviewer
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = username,
                });

                this.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region AddRestaurant
        public void AddRestaurant(string name, string city, string state, string address, string postalCode)
        {
            try
            {
                Restaurant restaurant = (from rest in this.Restaurant where rest.Name.ToLower() == name.Trim().ToLower() select rest).FirstOrDefault();
                if (restaurant == null)
                {
                    restaurant = new Restaurant
                    {
                        Name = name
                    };

                    this.Restaurant.Add(restaurant);
                    this.SaveChanges();
                }

                this.Address.Add(new Address
                {
                    City = city.Trim(),
                    State = state.Trim(),
                    PostalCode = postalCode.Trim(),
                    EntityId = restaurant.Id,
                });

                this.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region DoesRestaurantExist
        public bool DoesRestaurantExist(string name, string city, string state, string address, string postalCode)
        {
            try
            {
                Restaurant restCheck = (from rest in this.Restaurant where rest.Name.ToLower() == name.Trim().ToLower() select rest).FirstOrDefault();
                if (restCheck != null)
                {
                    Address addyCheck = (from addy in this.Address
                                         where addy.EntityId == restCheck.Id &&
                                               addy.City.ToLower() == city.Trim().ToLower() &&
                                               addy.State.ToLower() == state.Trim().ToLower() &&
                                               addy.PostalCode.ToLower() == postalCode.Trim().ToLower()
                                         select addy).FirstOrDefault();

                    if (addyCheck != null)
                        return true;
                    else
                        return false;
                }

                return false;
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
    #endregion

    #region DBInitializer
    public class DBInitializer : DropCreateDatabaseAlways<RRContext>
    {
        protected override void Seed(RRContext context)
        {
            base.Seed(context);

 
            var restaurant = new Restaurant()
            {
                CreatedDateTime = DateTime.Now,
                CreatedByUserId = "RRC",
                Name = "The Pie Hole",
            };

            context.Restaurant.Add(restaurant);
            context.SaveChanges();

            var address1 = new Address
            {
                Address1 = "444 Universal Drive",
                City = "New York",
                State = "NY",
                PostalCode = "11230",
                EntityId = restaurant.Id,
            };

            var address2 = new Address
            {
                Address1 = "42 Answer Road",
                City = "Raymore",
                State = "MO",
                PostalCode = "64083",
                EntityId = restaurant.Id,
            };

            context.Address.Add(address1);
            context.Address.Add(address2);
            context.SaveChanges();

            var reviewer1 = new Reviewer
            {
                 FirstName = "Rafet",
                 LastName = "Canovic",
                 UserName = "RCNYC"
            };

            var reviewer2 = new Reviewer
            {
                FirstName = "Rafet",
                LastName = "Canovic",
                UserName = "Fritz"
            };

            context.Reviewer.Add(reviewer1);
            context.Reviewer.Add(reviewer2);
            context.SaveChanges();

            var review1 = new Review 
            {
                    EntityId = restaurant.Id,
                    Rating = 5,
                    ReviewText = "Great place to eat.",
                    ReviewerId = reviewer1.Id,       
            };

            var review2 = new Review
            {
                EntityId = restaurant.Id,
                Rating = 5,
                ReviewText = "Amazing Pizza. Definitely a place to stuff your face.",
                ReviewerId = reviewer2.Id,
            };

            context.Review.Add(review1);
            context.Review.Add(review2);
            context.SaveChanges();

            context.ReviewLocation.Add(new ReviewLocation
            {
                AddressId = address1.Id,
                ReviewId = review1.Id,
            });

            context.ReviewLocation.Add(new ReviewLocation
            {
                AddressId = address2.Id,
                ReviewId = review2.Id,
            });

            context.SaveChanges();
        }
    }
    #endregion
}
