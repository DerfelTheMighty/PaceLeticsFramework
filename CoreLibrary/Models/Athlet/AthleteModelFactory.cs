﻿namespace CoreLibrary.Models.Athlet
{
	public class AthleteModelFactory
	{
		public List<AthleteModel> CreateAthleteModel() 
		{
			List<AthleteModel> list = new List<AthleteModel>();
			list.Add(new AthleteModel()
			{
				Name = "Humbug Hund",
				Id = Guid.NewGuid().ToString(),
				Level = Enums.ExperienceLevel.Intermediate,
				Vdot = 45.5
			}
			);
			list.Add(new AthleteModel()
			{
				Name = "Derfel Cadarn",
				Id = Guid.NewGuid().ToString(),
				Level = Enums.ExperienceLevel.Novice,
				Vdot = 50.5
			});
			list.Add(new AthleteModel()
			{
				Name = "Jesse Ventura",
				Id = Guid.NewGuid().ToString(),
				Level = Enums.ExperienceLevel.Expert,
				Vdot = 56.5
			});
			return list;


		}
	}
}
