using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MeshCreate : MonoBehaviour
{
    public GameObject roadPrefab; // Yol parçasý prefabý
    public GameObject yuvarlak; // Yol parçasý prefabý
    float roadWidth = 10f; // Yol parçasý geniþliði
    //float roadHeight = 0.1f; // Yol parçasý yüksekliði

    public void CreateRoad(Vector3 startPoint, Vector3 endPoint)
    {
        Instantiate(yuvarlak, startPoint + new Vector3(0, 0.005f, 0), yuvarlak.transform.rotation);
        float distance = Vector3.Distance(startPoint, endPoint); // Noktalar arasýndaki mesafe
        int roadPieces = Mathf.RoundToInt(distance / roadWidth); // Yol parçalarýnýn sayýsý

        Vector3 direction = (endPoint - startPoint).normalized; // Noktalar arasýndaki yön vektörü

        // Yol parçalarýný oluþtur
        for (int i = 0; i < roadPieces; i++)
        {
            Vector3 position = startPoint + direction * (i * roadWidth + roadWidth / 2f); // Yol parçasýnýn pozisyonu
            Quaternion rotation = Quaternion.LookRotation(direction); // Yol parçasýnýn rotasyonu

            // Yol parçasý prefabýný kopyala ve pozisyonunu/rotasyonunu ayarla
            GameObject roadPiece = Instantiate(roadPrefab, position + new Vector3(0, 0.005f, 0), rotation);
        }
    }

    public void WithEarClipping(Vector3[] vertices)
    {
        // Vertex sayýsýný al
        int verticesCount = vertices.Length;

        // Vertexlerin 2D koordinatlarýný tutacak dizi
        Vector2[] uv = new Vector2[verticesCount * 2];

        // Üçgenlerin dizisi
        int[] tringles;

        // Vertexlerin yükseklik deðerlerini kaldýrarak 2D koordinatlarýna çevir
        Vector2[] v2vertices = new Vector2[verticesCount];
        for (int i = 0; i < verticesCount; i++)
        {
            v2vertices[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        // Vertexlerin üçgenleþtirilmesi
        List<int> abc = Triangulation.Tringulate(v2vertices.ToList());

        // Eðer üçgenleþtirme baþarýlý olduysa
        if (abc != null)
        {
            // Vertexlerin yükseklik deðerlerini ekleyerek 3D koordinatlara çevir
            Vector3[] vertices2 = new Vector3[verticesCount * 2];
            Vector3 yükseklik = new Vector3(0, 10, 0);
            for (int i = 0; i < verticesCount; i++)
            {
                vertices2[i] = vertices[i];
                vertices2[i + verticesCount] = vertices[i] + yükseklik;
            }

            int a = abc.Count, b = verticesCount;
            List<int> temp = new List<int>();

            // Çatý için ilk üçgenleri tersten ekle
            for (int i = 0; i < a; i++)
            {
                temp.Add(abc[i] + b);
            }
            temp.Reverse();

            // Kenarlarýn üçgen listesine eklenmesi
            for (int i = 0; i < b - 1; i++)
            {
                temp.Add(i);
                temp.Add(i + b);
                temp.Add(i + b + 1);
                temp.Add(i + b + 1);
                temp.Add(i + 1);
                temp.Add(i);

                // Düzenlemeye ihtiyaç duyan hata giderme alaný
                temp.Add(i);
                temp.Add(i + 1);
                temp.Add(i + b + 1);
                temp.Add(i + b + 1);
                temp.Add(i + b);
                temp.Add(i);
            }

            // Kenara ait son üçgenlerin eklenmesi
            temp.Add(b - 1);
            temp.Add(b * 2 - 1);
            temp.Add(b);
            temp.Add(b);
            temp.Add(0);
            temp.Add(b - 1);

            // Düzenlemeye ihtiyaç duyan hata giderme alaný
            temp.Add(b - 1);
            temp.Add(0);
            temp.Add(b);
            temp.Add(b);
            temp.Add(b * 2 - 1);
            temp.Add(b - 1);

            // Üçgenlerin dizisini birleþtir
            abc.AddRange(temp);
            tringles = abc.ToArray();

            // Mesh nesnesi oluþtur
            Mesh mesh = new Mesh();
            mesh.vertices = vertices2;
            mesh.triangles = tringles;
            mesh.uv = uv;

            // Oyun nesnesi oluþtur ve mesh'i ekle
            GameObject gameObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));

            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            gameObject.GetComponent<Renderer>().material.color = Color.black;
        }
    }
}
