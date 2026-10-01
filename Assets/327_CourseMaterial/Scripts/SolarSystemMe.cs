// IMDM327 Material
// Use CSV or JSON to load data into the simulation. Both formats are supported, but they use different data types. 
// The CSV format uses a struct, while the JSON format uses a class. This script demonstrates how to load both formats and access their data.
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.UIElements;
public class SolarSystemMe : MonoBehaviour
{
    // These components can be attached independently.
    public new GameObject camera; 
    public int cameraDistance = 50;

    public long radiusScale = 500000; 
    public int speed = 10000000;
    public float distanceScale = 1e18f;
public float massScale = 1e24f;
    DataCSV solarCSV;
    DataJSON solarJSON;

   
    const float G = 6.674e-11f; // Gravitational constant
    PlanetProperty[] planetProperties;
    private int numberOfSphere = 10;
    class PlanetProperty // why struct?
    {                   // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
        public GameObject planet;
        public float mass;
        public float radius;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 actualPosition;
    }

    // CSV data and JSON data use their own data types.
    public BodyProperty[] solarBodiesCSV; 
    public SolarBody[] solarBodiesJSON;
    private float minimumDistance = 3f;

    // Both loader scripts finish reading their files in Awake().
    void Start()
    {
        camera.transform.position = new UnityEngine.Vector3(0, cameraDistance, 0);
        camera.transform.eulerAngles = new UnityEngine.Vector3(90, 0, 0);
        // CSV: use this block when a DataCSV component is attached.
        // solarCSV = GetComponent<DataCSV>();
        // if (solarCSV != null)
        // {
        //     solarBodiesCSV = solarCSV.bp;
        //     Debug.Log("Loaded " + solarBodiesCSV.Length + " bodies from solar.csv.");
        //     Debug.Log("First body: mass = " + solarBodiesCSV[0].mass + ", distance = " + solarBodiesCSV[0].distance + ", initial_velocity = " + solarBodiesCSV[0].initial_velocity);
        // }

        //JSON: use this block when a DataJSON component is attached.
        solarJSON = GetComponent<DataJSON>();
        if (solarJSON != null)
        {
            solarBodiesJSON = solarJSON.solarData.bodies;
            Debug.Log("Loaded " + solarBodiesJSON.Length + " bodies from solar.json.");
            Debug.Log("First body: " + solarBodiesJSON[0].name + ", mass: " + solarBodiesJSON[0].mass);
        }


        // GameObject array to hold the planets in the simulation.
        planetProperties = new PlanetProperty[numberOfSphere];
        for (int i = 0; i < numberOfSphere; i++)
        {
            // Our gameobjects are created here:
            planetProperties[i] = new PlanetProperty();
            planetProperties[i].planet = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
        }

        // Apply the loaded data to the simulation. This is where you would set up your bodies in the scene based on the loaded data.
        for (int i = 0; i < solarBodiesJSON.Length; i++)
        {
            float scaledSize = solarBodiesJSON[i].radius;
            float initVelocity = solarBodiesJSON[i].initial_velocity;
            float theta = Random.Range(0f, 2f * Mathf.PI);
            float r = solarBodiesJSON[i].distance;
            
            planetProperties[i].mass = solarBodiesJSON[i].mass;
            planetProperties[i].radius = scaledSize;
            planetProperties[i].actualPosition = new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));
            planetProperties[i].planet.transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);
            

            
            planetProperties[i].velocity = new Vector3(-initVelocity * Mathf.Sin(theta), 0f, initVelocity * Mathf.Cos(theta));
            Vector3 scaledDistance = planetProperties[i].actualPosition / distanceScale;
            // float newx;
            // float newz;
            // if (scaledDistance.x >= 0f)
            // {
            //     newx = Mathf.Pow(scaledDistance.x, 1f/2f);
            // }
            // else
            // {
            //     newx = Mathf.Pow(-1 * scaledDistance.x, 1f/2f) * -1;
            // }
            // if (scaledDistance.z >= 0f)
            // {
            //     newz = Mathf.Pow(scaledDistance.z, 1f/2f);
            // }
            // else
            // {
            //     newz = Mathf.Pow(-1 * scaledDistance.z, 1f/2f) * -1;
            // }
            // scaledDistance = new Vector3(newx, scaledDistance.y, newz);
            planetProperties[i].planet.transform.position = scaledDistance;
        


            // + This is just pretty trails
            TrailRenderer trailRenderer = planetProperties[i].planet.AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer.time = 1000.0f;  // Duration of the trail
            trailRenderer.startWidth = 5f;  // Width of the trail at the start
            trailRenderer.endWidth = 1f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // Set the colour gradient along the trail.
            Gradient gradient = new Gradient();
            Color targetColor = Color.HSVToRGB((float)i / numberOfSphere, 1f, 1f);
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f), // (color, normalized position)
                    new GradientColorKey(targetColor, 0.8f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f), // (alpha, normalized position) 
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trailRenderer.colorGradient = gradient;


        }

    }
    void Update()
    {
        // Loop for N-body gravity
        // How should we design the loop?

        // 00. Initialize the acceleration for each body to zero at the start of each frame
       for (int i = 0; i < numberOfSphere; i++)
        {
            // ***WRITE YOUR CODE HERE***
            planetProperties[i].acceleration = Vector3.zero;
        }
        // 01. Loop through each body to calculate the gravitational forces acting on it
        for (int i = 1; i < numberOfSphere; i++)
        {
            for (int j = 0; j < numberOfSphere; j++)
            {
                if (i == j)
                {
                    continue;
                }
                Vector3 distance = planetProperties[j].actualPosition - planetProperties[i].actualPosition;//get distance between the two
                Vector3 scaledD = distance;
                float scaledM1 = planetProperties[i].mass;
                float scaledM2 = planetProperties[j].mass;
                Vector3 gravity = CalculateGravity(scaledD, scaledM1, scaledM2); //calculate gravity between the two 
                planetProperties[i].acceleration += gravity / planetProperties[i].mass; //calculate acceleration
                //planetProperties[j].acceleration += gravity / planetProperties[j].mass; //calculate acceleration
                if (distance.magnitude < minimumDistance) //if two objects are closer than they should be
                {
                    planetProperties[i].acceleration += 3f * gravity /planetProperties[i].mass; // repel
                    planetProperties[j].acceleration -= 3f * gravity /planetProperties[j].mass; // repel
                }
            }
            
        }

        // 02. Loop through each body to update its velocity and position based on the calculated acceleration
       for (int i = 0; i < numberOfSphere; i++)
        {
            // ***WRITE YOUR CODE HERE***
            planetProperties[i].velocity += planetProperties[i].acceleration * 1000000f * Time.deltaTime;
            planetProperties[i].actualPosition += planetProperties[i].velocity * 1000000f * Time.deltaTime;
            // Scale: 
            Vector3 scaledDistance = planetProperties[i].actualPosition / distanceScale;
            // float newx;
            // float newz;
            // if (scaledDistance.x >= 0f)
            // {
            //     newx = Mathf.Pow(scaledDistance.x, 1f/2f);
            // }
            // else
            // {
            //     newx = Mathf.Pow(-1 * scaledDistance.x, 1f/2f) * -1;
            // }
            // if (scaledDistance.z >= 0f)
            // {
            //     newz = Mathf.Pow(scaledDistance.z, 1f/2f);
            // }
            // else
            // {
            //     newz = Mathf.Pow(-1 * scaledDistance.z, 1f/2f) * -1;
            // }
            // scaledDistance = new Vector3(newx, scaledDistance.y, newz);
            planetProperties[i].planet.transform.position = scaledDistance;

        }
    }

    // Gravity Fuction to finish
    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity = Vector3.zero; // note this is also Vector3
        gravity = G * m1 * m2 / (distanceVector.sqrMagnitude) * distanceVector.normalized;
        return gravity;
    }
}
