using Kbs.IdoWeb.Data.Location;
using Kbs.IdoWeb.Data.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace Kbs.IdoWeb.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LocationController : ControllerBase
	{
		private readonly LocationContext _locContext;
		private readonly MappingContext _mapContext;
		public LocationController(LocationContext locContext, MappingContext mapContext)
		{
			_locContext = locContext;
			_mapContext = mapContext;
		}

		[HttpPost("OsmCoordinates")]
		//POST: /api/Location/OsmCoordinates
		public ActionResult<string> PostOsmCoordinates(dynamic searchObject)
		{
			string searchText = "";
			try
			{
				searchText = searchObject.SearchText.Value;
			}
			catch (System.Exception)
			{
			}
			List<LocationItem> resultList = new List<LocationItem>();
			if (searchText.Length > 2)
			{
				try
				{
					var searchTextParam = new NpgsqlParameter("SearchText", searchText + "%");
					var request = _locContext.LocationItem.FromSql("Select Id,Name,z_order as Order,ST_Y(ST_Transform(geometry,4123)) as Lat,ST_X(ST_Transform(geometry,4123)) as Lon from \"Map\".osm_new_places Where z_order>2 AND LOWER(name) like LOWER(@SearchText) Order By name LIMIT 100;", searchTextParam);
					resultList = request.AsEnumerable()
						.Select(i => new LocationItem() { Id = i.Id, Name = i.Name, Order = i.Order, Lat = Math.Round(i.Lat, 6), Lon = Math.Round(i.Lon, 6) })
						.ToList();
				}
				catch (System.Exception ex)
				{
					throw ex;
				}
			}
			return Content(JsonConvert.SerializeObject(resultList), "application/json");
		}

		/**@TODO: rewrite to webmethod **/
		[HttpGet("OsmLocation")]
		//GET: /api/Location/OsmLocation
		public ActionResult<string> GetOsmLocation(double lat, double lon, int? order)
		{
			LocationItem result = new LocationItem();
			try
			{
				var pointParam = new NpgsqlParameter("point", Invariant($"POINT({lon} {lat})"));
				IQueryable<LocationItem> request;
				if (order == null)
				{
					request = _locContext.LocationItem.FromSql("Select Id,Name,z_order as Order,ST_Y(ST_Transform(geometry,4123)) as Lat,ST_X(ST_Transform(geometry,4123)) as Lon, ST_Distance(ST_Transform(ST_GeomFromText(@Point,4123),3857),geometry) as Distance from \"Map\".osm_new_places Where z_order>2 AND z_order<6 order by Distance asc Limit 1", pointParam);
				}
				else
				{
					var orderParam = new NpgsqlParameter("order", order);
					request = _locContext.LocationItem.FromSql("Select Id,Name,z_order as Order,ST_Y(ST_Transform(geometry,4123)) as Lat,ST_X(ST_Transform(geometry,4123)) as Lon, ST_Distance(ST_Transform(ST_GeomFromText(@Point,4123),3857),geometry) as Distance from \"Map\".osm_new_places Where z_order=@order order by Distance asc Limit 1", pointParam,orderParam);
				}
				result = request.AsEnumerable()
					.Select(i => new LocationItem() { Id = i.Id, Name = i.Name, Order = i.Order, Lat = Math.Round(i.Lat, 6), Lon = Math.Round(i.Lon, 6) })
					.First();

			}
			catch (System.Exception ex)
			{
				throw ex;

			}
			return Content(JsonConvert.SerializeObject(result), "application/json");

		}
	}
}