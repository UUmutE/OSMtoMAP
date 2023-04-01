using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Linq;

public class OSMReader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Document();
    }
    public GameObject zeminObje;
    public string mapPath;
    public void Document()
    {
        XmlDocument d = new XmlDocument();
        d.Load(Application.dataPath + mapPath);

        XmlNodeList wayNodes = d.SelectNodes("osm/way");
        XmlNodeList nodes = d.SelectNodes("osm/node");
        XmlElement bounds = d.SelectSingleNode("osm/bounds") as XmlElement;

        double maxlat = Convert.ToDouble(bounds.GetAttribute("maxlat"), System.Globalization.CultureInfo.InvariantCulture);
        double maxlon = Convert.ToDouble(bounds.GetAttribute("maxlon"), System.Globalization.CultureInfo.InvariantCulture);
        double minlat = Convert.ToDouble(bounds.GetAttribute("minlat"), System.Globalization.CultureInfo.InvariantCulture);
        double minlon = Convert.ToDouble(bounds.GetAttribute("minlon"), System.Globalization.CultureInfo.InvariantCulture);

        double maxY = MercatorProjection.latToY(maxlat);
        double maxX = MercatorProjection.lonToX(maxlon);
        double minY = MercatorProjection.latToY(minlat);
        double minX = MercatorProjection.lonToX(minlon);
        double genX = maxX - minX;
        double genY = maxY - minY;
        Vector3 merkez = new Vector3((float)((maxX + minX) / 2-minX), 0, (float)((maxY + minY) / 2-minY));
        GameObject zemin = Instantiate(zeminObje, merkez, zeminObje.transform.rotation);
        zemin.transform.localScale = new Vector3((float)genX/10, 1, (float)genY/10);
        zemin.GetComponent<Renderer>().material.color = Color.gray;
        zemin.gameObject.isStatic = true;

        Dictionary<string, XmlElement> nodeKV = new Dictionary<string, XmlElement>();
        foreach (XmlElement node in nodes)
        {
            nodeKV.Add((node as XmlElement).GetAttribute("id"), node);
        }
        foreach (XmlNode wayNode in wayNodes)
        {
            XmlNodeList tagList = wayNode.SelectNodes("tag");
            foreach (XmlElement tag in tagList)
            {
                if(tag.GetAttribute("k") == "building")
                {
                    XmlNodeList ndList = tag.ParentNode.SelectNodes("nd");
                    Vector3[] vertices = new Vector3[ndList.Count-1];
                    for (int i = 0; i < ndList.Count-1; i++)
                    {
                        XmlElement nd = ndList[i] as XmlElement;
                        double[] XY = MercatorProjection.toPixel(Convert.ToDouble(nodeKV[nd.GetAttribute("ref")].GetAttribute("lon"),
                                                        System.Globalization.CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(nodeKV[nd.GetAttribute("ref")].GetAttribute("lat"),
                                                        System.Globalization.CultureInfo.InvariantCulture));

                        XY[0] -= minX;
                        XY[1] -= minY;

                        double tempx = XY[0] % 0.00001;
                        double tempy = XY[1] % 0.00001;
                        XY[0] -= tempx;
                        XY[1] -= tempy;

                        vertices[i] = new Vector3((float)XY[0], 0, (float)XY[1]);
                    }
                    gameObject.GetComponent<MeshCreate>().WithEarClipping(vertices);
                }

                if (tag.GetAttribute("k") == "highway")
                {
                    XmlNodeList ndList = tag.ParentNode.SelectNodes("nd");
                    for (int i = 0; i < ndList.Count - 1; i++)
                    {
                        XmlElement ndS = ndList[i] as XmlElement;

                        double[] XYS = MercatorProjection.toPixel(Convert.ToDouble(nodeKV[ndS.GetAttribute("ref")].GetAttribute("lon"),
                                                        System.Globalization.CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(nodeKV[ndS.GetAttribute("ref")].GetAttribute("lat"),
                                                        System.Globalization.CultureInfo.InvariantCulture));

                        XYS[0] -= minX;
                        XYS[1] -= minY;

                        double tempx = XYS[0] % 0.00001;
                        double tempy = XYS[1] % 0.00001;
                        XYS[0] -= tempx;
                        XYS[1] -= tempy;


                        XmlElement ndF = ndList[i + 1] as XmlElement;

                        double[] XYF = MercatorProjection.toPixel(Convert.ToDouble(nodeKV[ndF.GetAttribute("ref")].GetAttribute("lon"),
                                                        System.Globalization.CultureInfo.InvariantCulture),
                                                        Convert.ToDouble(nodeKV[ndF.GetAttribute("ref")].GetAttribute("lat"),
                                                        System.Globalization.CultureInfo.InvariantCulture));

                        XYF[0] -= minX;
                        XYF[1] -= minY;

                        tempx = XYF[0] % 0.00001;
                        tempy = XYF[1] % 0.00001;
                        XYF[0] -= tempx;
                        XYF[1] -= tempy;

                        gameObject.GetComponent<MeshCreate>().CreateRoad
                            (new Vector3((float)XYS[0], 0, (float)XYS[1]), new Vector3((float)XYF[0], 0, (float)XYF[1]));

                    }
                }
            }
        }
    }
}