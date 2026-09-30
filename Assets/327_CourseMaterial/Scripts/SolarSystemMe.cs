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
    public int cameraDistance = 3000;
    public long distanceScale = 1000000000; 
    public long radiusScale = 500000; 
    public int speed = 1000000;
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

    // Both loader scripts finish reading their files in Awake().
    void Start()
    {
        camera.transform.position = new UnityEngine.Vector3(0, cameraDistance, 0);
        camera.transform.eulerAngles = new UnityEngine.Vector3(90, 0, 0);
        // CSV: use this block when a DataCSV component is attached.
        solarCSV = GetComponent<DataCSV>();
        if (solarCSV != null)
        {
            solarBodiesCSV = solarCSV.bp;
            Debug.Log("Loaded " + solarBodiesCSV.Length + " bodies from solar.csv.");
            Debug.Log("First body: mass = " + solarBodiesCSV[0].mass + ", distance = " + solarBodiesCSV[0].distance + ", initial_velocity = " + solarBodiesCSV[0].initial_velocity);
        }

        // JSON: use this block when a DataJSON component is attached.
        // solarJSON = GetComponent<DataJSON>();
        // if (solarJSON != null)
        // {
        //     solarBodiesJSON = solarJSON.solarData.bodies;
        //     Debug.Log("Loaded " + solarBodiesJSON.Length + " bodies from solar.json.");
        //     Debug.Log("First body: " + solarBodiesJSON[0].name + ", mass: " + solarBodiesJSON[0].mass);
        // }


        // GameObject array to hold the planets in the simulation.
        planetProperties = new PlanetProperty[numberOfSphere];
        for (int i = 0; i < numberOfSphere; i++)
        {
            
            // Our gameobjects are created here:
            planetProperties[i] = new PlanetProperty();
            planetProperties[i].planet = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
        }

        // Apply the loaded data to the simulation. This is where you would set up your bodies in the scene based on the loaded data.
        for (int i = 0; i < solarBodiesCSV.Length; i++)
        {
            float scaledSize = Mathf.Clamp((float)(solarBodiesCSV[i].radius / radiusScale), 0, 500000 / radiusScale/25000);
            float initVelocity = solarBodiesCSV[i].initial_velocity;
            float theta = Random.Range(0f, 2f * Mathf.PI);
            float r = solarBodiesCSV[i].distance;
            
            planetProperties[i].mass = solarBodiesCSV[i].mass;
            planetProperties[i].radius = scaledSize;
            planetProperties[i].actualPosition = new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));
            planetProperties[i].planet.transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);
            

            
            planetProperties[i].velocity = new Vector3(-initVelocity * Mathf.Sin(theta), 0f, initVelocity * Mathf.Cos(theta));
            float scaledDistance = Mathf.Sqrt(r / 1e8f);
            planetProperties[i].planet.transform.position = 
            new Vector3((scaledDistance) * Mathf.Cos(theta), 0f, (scaledDistance) * Mathf.Sin(theta));
        





            // + This is just pretty trails
            TrailRenderer trailRenderer = planetProperties[i].planet.AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer.time = 100.0f;  // Duration of the trail
            trailRenderer.startWidth = 0.5f;  // Width of the trail at the start
            trailRenderer.endWidth = 0.1f;    // Width of the trail at the end
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
        for (int i = 0; i < numberOfSphere; i++)
        {
            Vector3 force = Vector3.zero;
            // ***WRITE YOUR CODE HERE***
            for (int j = 0; j < numberOfSphere; j++)
            {
                if (i == j)
                {
                    continue;
                }
                //force = force + (CalculateGravity(planetProperties[i].actualPosition, planetProperties[i].mass, planetProperties[j].mass) / planetProperties[i].mass);
                Vector3 distance =
                planetProperties[j].actualPosition -
                planetProperties[i].actualPosition;

                Vector3 gravity = CalculateGravity(
                    distance,
                    planetProperties[i].mass,
                    planetProperties[j].mass
                );

                force += gravity;
                // if (distance.magnitude < minimumDistance) //if two objects are closer than they should be
                // {
                //     planetProperties[i].acceleration -= 3f * gravity /planetProperties[i].mass; // repel
                //     planetProperties[j].acceleration += 3f * gravity /planetProperties[j].mass; // repel
                // }
            }
            planetProperties[i].acceleration = force / planetProperties[i].mass;
        }
        // 02. Loop through each body to update its velocity and position based on the calculated acceleration
       for (int i = 0; i < numberOfSphere; i++)
        {
            // ***WRITE YOUR CODE HERE***
            planetProperties[i].velocity += planetProperties[i].acceleration * Time.deltaTime;
            planetProperties[i].actualPosition += planetProperties[i].velocity * Time.deltaTime;
            // Scale: 
            float scaledDistance = Mathf.Sqrt(planetProperties[i].actualPosition.magnitude / 1e8f);
            planetProperties[i].planet.transform.position = planetProperties[i].actualPosition.normalized;

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
