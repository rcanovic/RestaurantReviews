using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

using RestaurantReviews.Context;
using SoftWriters.Models;

namespace RestaurantReviews.Controllers
{
    public class ReviewsController : ApiController
    {
        private RRContext db = new RRContext();

        #region GetAllRestaurantsAndReviews
        [Route("Reviews/GetAllRestaurantsAndReviews")]
        [HttpGet]
        public IHttpActionResult GetAllRestaurantsAndReviews()
        {
            return Ok(db.GetAllRestaurantsAndReviews());
        }
        #endregion

        #region GetAllRestaurantsByCity
        [Route("Reviews/GetAllRestaurantsByCity/{city}")]
        [HttpGet]
        public IHttpActionResult GetAllRestaurantsByCity(string city)
        {
            return Ok(db.GetAllRestaurantsByCity(city));
        }
        #endregion

        #region GetAllRestaurantReviewsByUser
        [Route("Reviews/GetAllRestaurantReviewsByUser/{user}")]
        [HttpGet]
        public IHttpActionResult GetAllRestaurantReviewsByUser(string user)
        {
            return Ok(db.GetAllRestaurantReviewsByUser(user));
        }
        #endregion

        #region AddReview
        [Route("Reviews/AddReview/{reviewerId}/{reviewText}/{rating}/{restaurantId}/{restAddressId}")]
        [HttpPost]
        public HttpResponseMessage AddReview(string reviewerId, string reviewText, string rating, string restaurantId, string restAddressId)
        {

            if (String.IsNullOrEmpty(reviewerId) || !int.TryParse(reviewerId, out int outRevId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for reviewerId.");
            }

            int revuerId = int.Parse(reviewerId);
            if (!db.Reviewer.Any(r => r.Id == revuerId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Selected reviewer was not found.");
            }

            if (String.IsNullOrEmpty(restaurantId) && !int.TryParse(restaurantId, out int outRestId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for restaurantId.");
            }

            int restId = int.Parse(restaurantId);
            var rest = db.Restaurant.Select(r => r.Id == restId);
            if (rest == null)
            {
                var message = string.Format("Restuarant with the Id '{0}' was not found.", restaurantId);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
            }

            if (String.IsNullOrEmpty(restAddressId) || !int.TryParse(restAddressId, out int outAddyId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for restAddressId.");
            }

            int restAddyId = int.Parse(restAddressId);
            if (!db.Address.Any(a => a.Id == restAddyId && a.EntityId == restId))
            {
                var message = string.Format("City with the address id of '{0}' was not found for this restaurant.", restAddressId);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
            }

            db.AddReview(revuerId, restId, restAddyId, reviewText, rating);
            return Request.CreateResponse(HttpStatusCode.Created);
        }
        #endregion

        #region DeleteReview
        [Route("Reviews/DeleteReview/{reviewId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteReview(string reviewId)
        {
            if (String.IsNullOrEmpty(reviewId) || !int.TryParse(reviewId, out int outRevId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for reviewId.");
            }

            int revuId = int.Parse(reviewId);
            if (!db.Review.Any(r => r.Id == revuId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Selected review was not found.");
            }

            db.DeleteReview(revuId);
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }
        #endregion

        #region AddReviewUser
        [Route("Reviews/AddReviewUser/{username}/{firstName}/{lastName}")]
        [HttpPost]
        public HttpResponseMessage AddReviewUser(string username, string firstName, string lastName)
        {
            if (String.IsNullOrEmpty(username))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for username.");
            }

            if (String.IsNullOrEmpty(firstName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for firstName.");
            }

            if (String.IsNullOrEmpty(lastName))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for lastName.");
            }

            if(db.Reviewer.Any(r => r.UserName.ToLower() == username.Trim().ToLower()))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username already exists.");
            }

            db.AddReviewUser(username, firstName, lastName);          
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }
        #endregion

        #region AddRestaurant
        [Route("Reviews/AddRestaurant/{name}/{city}/{state}/{address}/{postalCode}")]
        [HttpPost]
        public HttpResponseMessage AddRestaurant(string name, string city, string state, string address, string postalCode)
        {
            if (String.IsNullOrEmpty(name))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for name.");
            }

            if (String.IsNullOrEmpty(city))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for city.");
            }

            if (String.IsNullOrEmpty(state))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for state.");
            }

            if (String.IsNullOrEmpty(address))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for address.");
            }

            if (String.IsNullOrEmpty(postalCode))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "A value is required for postalCode.");
            }

            if (db.DoesRestaurantExist(name, city, state, address, postalCode))
            {               
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Restaurant already exists for this address.");               
            }

            db.AddRestaurant(name, city, state, address, postalCode);
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }
        #endregion
    }
}
