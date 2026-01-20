using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class StadiumModel
{
    public int id;             
    public string name;          
    public string address;        
    public Vector2 location;       
    public float rating;           
    public int reviewsCount;        
    public float basePricePerHour; 
    public string photoPath;
    public List<PitchSize> supportedSizes;

    [NonSerialized]
    private Sprite _photoCache;

    public Sprite photo
    {
        get
        {
            if (_photoCache == null && !string.IsNullOrEmpty(photoPath))
            {
                _photoCache = FileUtils.LoadImageAsSprite(photoPath);
            }
            return _photoCache;
        }
        set
        {
            _photoCache = value;
        }
    }

    public StadiumModel(PitchConfig config)
    {
        id = config.id;
        name = config.name;
        address = config.address;
        location = config.location;
        rating = config.rating;
        reviewsCount = config.reviewsCount;
        basePricePerHour = config.basePricePerHour;
        supportedSizes = new List<PitchSize>(config.supportedSizes);
        
        if (config.photo != null)
        {
            string fileName = $"stadium_{id}_photo.png";
            FileUtils.SaveImage(config.photo, fileName);
            photoPath = fileName;
            _photoCache = config.photo;
        }
    }
}
