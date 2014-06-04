using System;
using UnityEngine;

public class Sensor
{
    GameObject node;
    int direction = 0;
    int range = 64;

    public Sensor(GameObject node, int direction, int range)
    {
        this.node = node;
        this.direction = direction;
        this.range = range;
    }

    public float getSensorData()
    {
        bool result = true;
        Color color = Color.red;

        Vector3 sensorDirection;
        switch (direction) {
            case 1:
                sensorDirection = node.transform.right;
                break;
            case 2:
                sensorDirection = -node.transform.right;
                break;
            case 3:
                sensorDirection = node.transform.up;
                break;
            case 4:
                sensorDirection = -node.transform.up;
                color = Color.green;
                break;
            default:
                sensorDirection = node.transform.forward;
                color = Color.magenta;
                break;
        }

        int distance = range / 2;
        int start = 0;
        int end = range;
        while (end >= start) {
            distance = (start + end) / 2;
            if (distance <= 0)
                break;
            result = Physics.Raycast(node.transform.position, sensorDirection, distance);
            if (result)
            {
                end = distance - 1;
            }
            else
            {
                start = distance + 1;
            }
        }

        if (distance < range)
            Debug.DrawRay(node.transform.position, sensorDirection * distance, color);
        
        float data = ((float)(range - distance)) / range;
        return data;
    }
}
