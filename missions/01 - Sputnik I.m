Mission
{
    name = Sputnik I
    description = A new government initiative has provided Kerbin scientists a chance to explore space, and we've elected you as our lead engineer. Before we can do anything more complex, we'll need to collect some data on Kerbin's upper atmosphere. Construct a rocket that is able to leave the atmosphere, collect readings, and return to the surface of Kerbin with the sensors intact so we can collect their recorded data.

    reward = 50000
    category = ORBIT, PROBE

    packageOrder = 1

    SubMissionGoal
    {
        description = Reach an altitude of at least 70km and record the required atmospheric data. You will need 3 Communotron 16, 1 accelerometer, 1 gravimeter, 1 thermometer and 1 barometer.

        OrbitGoal
        {
            body = Kerbin
            minAltitude = 70000
        }

        PartGoal
        {
            partName = longAntenna
            partCount = 3
        }

        PartGoal
        {
            partName = sensorAccelerometer
            partCount = 1
        }

        PartGoal
        {
            partName = sensorBarometer
            partCount = 1
        }

        PartGoal
        {
            partName = sensorGravimeter
            partCount = 1
        }

        PartGoal
        {
            partName = sensorThermometer
            partCount = 1
        }
    }

    LandingGoal
    {
        body = Kerbin
    }
}
