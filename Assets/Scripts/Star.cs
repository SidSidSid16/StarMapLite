using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Vector3 = UnityEngine.Vector3;

public class Star : MonoBehaviour
{
    /// <summary>
    /// Identifier (HIP number). "HIP" is appended to the ID, for Hipparcos
    /// </summary>
    [SerializeField] public int ID; // { get; set; }
    /// <summary>
    /// Right Ascension (J2000) in degrees. 
    /// </summary>
    [SerializeField] public double RA; //{ get; set; }
    /// <summary>
    /// Declination (J2000) in degrees. 
    /// </summary>
    [SerializeField] public double DE; //{ get; set; }
    /// <summary>
    /// Magnitude in Johnson V
    /// </summary>
    [SerializeField] public double Vmag; //{ get; set; }
    /// <summary>
    /// Trigonometric parallax, measured in milli-seconds of arc
    /// </summary>
    [SerializeField] public double Plx; //{ get; set; }
    /// <summary>
    /// Colour index in Johnson B-V colour
    /// </summary>
    [SerializeField] public double CI; //{ get; set; }
    /// <summary>
    /// Distance of the star from the centre of Earth measured in kilometres
    /// </summary> 
    [SerializeField] public double distanceFromEarth;
    /// <summary>
    /// Cartesian (X,Y,Z) positioning of the star
    /// </summary>
    [SerializeField] Vector3 cartesianPositioning;
    /// <summary>
    /// The estimated surface temperature of the star measured in Kelvin
    /// </summary>
    [SerializeField] double surfaceTemperature;

    public bool AnimationEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        positionStar();
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }

    private void OnBecameVisible()
    {
        gameObject.SetActive(true);
    }


    private void distanceFromEarthCalc()
    {
        // Convert Plx from milliarcseconds to arcseconds (seconds of arc)
        //double PlxSOA = Plx / 1000;
        // Calculate distance from equation d=1/p
        // distance d is measured in parsecs and the parallax angle p is measured in arcseconds.
        // double dPC = 1 / PlxSOA;
        /// Convert parsecs to kilomentres
        /// Conversion can bbe found here:
        /// https://en.wikipedia.org/wiki/Parsec
        /// 
        /// QUICK NOTE: converting to km causes issues with unity - values are too high
        ///             maybe work with parsecs instead?
        ///             
        //distanceFromEarth = (dPC * (96939420213600000 / math.PI_DBL)) / 1000;
        //distanceFromEarth = distanceFromEarth / 10000;
        //distanceFromEarth = dPC * 50;
        //distanceFromEarth = dPC * 10; // multiplied by 10 to move stars away from camera
        distanceFromEarth = 999;
    }


    private void cartesianPositioningCalc()
    {
        distanceFromEarthCalc();

        double RA_rad = RA * (math.PI_DBL / 180);
        double DE_rad = DE * (math.PI_DBL / 180);

        cartesianPositioning.x = (float)-(distanceFromEarth * (math.cos(DE_rad)) * (math.cos(RA_rad)));
        cartesianPositioning.y = (float)(distanceFromEarth * (math.cos(DE_rad)) * (math.sin(RA_rad)));
        cartesianPositioning.z = (float)(distanceFromEarth * (math.sin(DE_rad)));
    }

    private void positionStar()
    {
        double radius;
        cartesianPositioningCalc();
        gameObject.transform.position = cartesianPositioning;
        //// Highlight the Ursa Major and Minor Constellations
        //if ((ID == 54061) || (ID == 53910) || (ID == 58001) || (ID == 59774) || (ID == 62956) || (ID == 65378) || (ID == 67301) || (ID == 11767) || (ID == 85822) || (ID == 82080) || (ID == 77055) || (ID == 79822) || (ID == 76008) || (ID == 84535))
        //{
        //    gameObject.transform.localScale = new Vector3(10, 10, 10);
        //}
        //else
        //{
        //    gameObject.transform.localScale = new Vector3(2, 2, 2);
        //}
        // double radiusPC = 3.2375E-14;
        // double PlxSOA = (Plx / 1000);
        // Calculate distance from equation d=1/p
        // distance d is measured in parsecs and the parallax angle p is measured in arcseconds.
        // double dPC = (1 / PlxSOA);

        //double absMag = Vmag - math.log10(math.pow(PlxSOA / radiusPC, 5));
        //double NormAbsMag = absMag + 66.81132179;
        //surfaceTemperature = 8540 / (CI + 0.865);
        //double relativeRadius = math.pow((5800 / surfaceTemperature), 2) * math.sqrt(math.pow(2.512, (4.83 - NormAbsMag)));

        // radius = 10 * math.pow(math.E, (-1.44 - Vmag) / 5);
        radius = 50 * math.pow(10, (-1.44 - Vmag) / 5);
        // radius = ((8 * (Vmag - 14.08)) / (-1.44 - 14.08)) + 2;
        // radius = math.sqrt(((24 * (Vmag - 14.08)) / (-1.44 - 14.08)) + 1);

        // radius = relativeRadius * radiusSun;

        gameObject.transform.localScale = new Vector3((float)radius, (float)radius, (float)radius);
    }
}
