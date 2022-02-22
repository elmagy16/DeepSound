using System;
using AutoMapper;
using DeepSound.SQLite;
using DeepSoundClient.Classes.Common;
using DeepSoundClient.Classes.Global;
using DeepSoundClient.Classes.User;

namespace DeepSound.Helpers.Utils
{
    public static class ClassMapper
    {
        public static IMapper Mapper;
        public static void SetMappers()
        {
            try
            {
                var configuration = new MapperConfiguration(cfg =>
                {
                    try
                    {
                        cfg.AllowNullCollections = true;

                        cfg.CreateMap<OptionsObject.Data, DataTables.SettingsTb>().ForMember(x => x.AutoIdSettings, opt => opt.Ignore());
                        cfg.CreateMap<UserDataObject, DataTables.InfoUsersTb>().ForMember(x => x.AutoIdInfoUsers, opt => opt.Ignore());
                        cfg.CreateMap<GenresObject.DataGenres, DataTables.GenresTb>().ForMember(x => x.AutoIdGenres, opt => opt.Ignore());
                        cfg.CreateMap<PricesObject.DataPrice, DataTables.PriceTb>().ForMember(x => x.AutoIdPrice, opt => opt.Ignore());
                        cfg.CreateMap<SoundDataObject, DataTables.SharedTb>().ForMember(x => x.AutoIdShared, opt => opt.Ignore());
                        cfg.CreateMap<SoundDataObject, DataTables.LatestDownloadsTb>().ForMember(x => x.AutoIdaLatestDownloads, opt => opt.Ignore()).ForAllMembers(opt => opt.Condition(srs => !string.IsNullOrEmpty(srs.Id.ToString())));

                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                // only during development, validate your mappings; remove it before release
                //configuration.AssertConfigurationIsValid();

                Mapper = configuration.CreateMapper();

                var cfg = new MapperConfigurationExpression
                {
                    AllowNullCollections = true
                }; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}