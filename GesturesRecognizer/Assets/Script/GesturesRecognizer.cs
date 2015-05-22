/**
* The $1 Gesture Recognizer
*
*		Jacob O. Wobbrock
* 		The Information School
*		University of Washington
*		Mary Gates Hall, Box 352840
*		Seattle, WA 98195-2840
*		wobbrock@u.washington.edu
*
*		Andrew D. Wilson
*		Microsoft Research
*		One Microsoft Way
*		Redmond, WA 98052
*		awilson@microsoft.com
*
*		Yang Li
*		Department of Computer Science and Engineering
* 		University of Washington
*		The Allen Center, Box 352350
*		Seattle, WA 98195-2840
* 		yangli@cs.washington.edu
*
*		Actionscript conversion: Christoph Ketzler christoph@ketzler.de
*		
*		Unity3D C# conversion: Zou Liang zouliang.cn@gmail.com
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GesturesRecognizer
{
	//点结构体
	public struct Point
	{
		public double x,y;
		public Point(double _x, double _y){
			x = _x;
			y = _y;
		}
	}
	//范围框结构体
	public struct Rectangle
	{
		public double x,y,width,height;
		public Rectangle(double _x,double _y,double _w, double _h){
			x      = _x;
			y      = _y;
			width  = _w;
			height = _h;
		}
	}
	//返回结果结构体
	public struct Result
	{
		public string Name;
		public double Score;
		public Result(string _name, double _score){
			Name  = _name;
			Score = _score;
		}
	}
	//手势模板结构体
	public struct Template
	{
		public string Name;
		public List<Point> Points;
		public Template(string _name, List<Point> _point){
			Name  = _name;
			Points = _point;
		}
	}
	
	public static int NumPoints = 64;
	public static double SquareSize = 250.0;
	public static double HalfDiagonal = 0.5 * Math.Sqrt(250.0 * 250.0 + 250.0 * 250.0);
	public static double AngleRange = 45.0;
	public static double AnglePrecision = 2.0;
	public static double Phi = 0.5 * (-1.0 + Math.Sqrt(5.0)); // Golden Ratio
	public List<Template> Templates;
	
	
	//在自定义类中构造函数可以用
	public GesturesRecognizer(){
		Templates = new List<Template>();
		this.addTemplate("Circle", new List<Point>(){new Point(127,141),new Point(124,140),new Point(120,139),new Point(118,139),new Point(116,139),new Point(111,140),new Point(109,141),new Point(104,144),new Point(100,147),new Point(96,152),new Point(93,157),new Point(90,163),new Point(87,169),new Point(85,175),new Point(83,181),new Point(82,190),new Point(82,195),new Point(83,200),new Point(84,205),new Point(88,213),new Point(91,216),new Point(96,219),new Point(103,222),new Point(108,224),new Point(111,224),new Point(120,224),new Point(133,223),new Point(142,222),new Point(152,218),new Point(160,214),new Point(167,210),new Point(173,204),new Point(178,198),new Point(179,196),new Point(182,188),new Point(182,177),new Point(178,167),new Point(170,150),new Point(163,138),new Point(152,130),new Point(143,129),new Point(140,131),new Point(129,136),new Point(126,139)});
		this.addTemplate("V", new List<Point>(){new Point(0,500),new Point(200,300),new Point(400,500)});
		this.addTemplate("X", new List<Point>(){new Point(100,100),new Point(0,0),new Point(0,100),new Point(100,0)});
	}
	
	public Result Recognize(List<Point> points){
		
		points = Resample(points, NumPoints);//重采样
		points = RotateToZero(points);//旋转回零角度
		points = ScaleToSquare(points, SquareSize);//缩放到单位大小
		points = TranslateToOrigin(points);//移动到原点
		double b = +Mathf.Infinity; //正无穷大
		int t = 0;
		for (int i = 0; i < this.Templates.Count; i++)
		{
			double d = DistanceAtBestAngle(points, this.Templates[i], -AngleRange, +AngleRange, AnglePrecision);
			if (d < b)
			{
				b = d;
				t = i;
			}
		}
		double score = 1.0f - (b / HalfDiagonal);
		return new Result(this.Templates[t].Name, score);
	}
	
	/// <summary>
	/// Adds the template.
	/// </summary>
	/// <returns>The template.</returns>
	/// <param name="_name">_name.</param>
	/// <param name="_point">_point.</param>
	public int addTemplate(string _name , List<Point> _point){
		_point = Resample(_point, NumPoints);//重采样
		_point = RotateToZero(_point);//旋转回零角度
		_point = ScaleToSquare(_point, SquareSize);//缩放到单位大小
		_point = TranslateToOrigin(_point);//移动到原点
		this.Templates.Add( new Template(_name, _point)); // append new template
		int num = 0;
		for (int i = 0; i < this.Templates.Count; i++)
		{
			if (this.Templates[i].Name == _name)
				num++;
		}
		return num;
	}
	
	
	
	/// <summary>
	/// Resample the specified points and n.
	/// </summary>
	/// <param name="points">Points.</param>
	/// <param name="n">N.</param>
	public static List<Point> Resample(List<Point> points, int n){
		double I = PathLength(points) / (n - 1); // interval length  平均间距
		double D = 0.0;
		List<Point> newpoints = new List<Point>(){points[0]};
		for (int i = 1; i < points.Count; i++)
		{
			double d = Distance(points[i - 1], points[i]);
			if ((D + d) >= I)
			{
				double qx = points[i - 1].x + ((I - D) / d) * (points[i].x - points[i - 1].x);
				double qy = points[i - 1].y + ((I - D) / d) * (points[i].y - points[i - 1].y);
				Point q = new Point(qx, qy);
				newpoints.Add(q); // append new point 'q'
				points.Insert(i, q); // insert 'q' at position i in points s.t. 'q' will be the next i
				D = 0.0;
			}
			else D += d;
		}
		// somtimes we fall a rounding-error short of adding the last point, so add it if so
		if (newpoints.Count == n - 1)
		{
			newpoints.Add(points[points.Count - 1]);
		}
		return newpoints;
	}
	public static double PathLength(List<Point> points){
		double length = 0;
		for (int i = 1; i < points.Count; i++)
		{
			length += Distance(points[i - 1], points[i]);
		}
		return length;
	}
	public static double Distance(Point p1, Point p2){
		double dx = p2.x - p1.x;
		double dy = p2.y - p1.y;
		return Math.Sqrt(dx * dx + dy * dy);
	}
	
	/// <summary>
	/// Rotates to zero.
	/// </summary>
	/// <returns>The to zero.</returns>
	/// <param name="points">Points.</param>
	public static List<Point> RotateToZero(List<Point> points){
		Point c = Centroid(points);
		double theta = Math.Atan2(c.y - points[0].y, c.x - points[0].x);
		return RotateBy(points, -theta);
	}
	public static Point Centroid(List<Point> points)
	{
		double x = 0.0, y = 0.0;
		for (int i = 0; i < points.Count; i++)
		{
			x += points[i].x;
			y += points[i].y;
		}
		x /= points.Count;
		y /= points.Count;
		return new Point(x, y);
	}
	public static List<Point> RotateBy(List<Point> points, double theta) 
	{
		Point c = Centroid(points);
		double cos = Math.Cos(theta);
		double sin = Math.Sin(theta);
		
		List<Point> newpoints = new List<Point>();
		for (int i = 0; i < points.Count; i++)
		{
			double qx = (points[i].x - c.x) * cos - (points[i].y - c.y) * sin + c.x;
			double qy = (points[i].x - c.x) * sin + (points[i].y - c.y) * cos + c.y;
			newpoints.Add(new Point(qx, qy));
		}
		return newpoints;
	}
	
	/// <summary>
	/// Scales to square.
	/// </summary>
	/// <returns>The to square.</returns>
	/// <param name="points">Points.</param>
	/// <param name="size">Size.</param>
	public static List<Point> ScaleToSquare(List<Point> points, double size){
		Rectangle B = BoundingBox(points);
		List<Point> newpoints = new List<Point>();
		for (var i = 0; i < points.Count; i++)
		{
			double qx = points[i].x * (size / B.width);
			double qy = points[i].y * (size / B.height);
			newpoints.Add(new Point(qx, qy));
		}
		return newpoints;
	}	
	public static Rectangle BoundingBox(List<Point> points){
		double minX = +Mathf.Infinity, maxX = -Mathf.Infinity, minY = +Mathf.Infinity, maxY = -Mathf.Infinity;
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].x < minX)
				minX = points[i].x;
			if (points[i].x > maxX)
				maxX = points[i].x;
			if (points[i].y < minY)
				minY = points[i].y;
			if (points[i].y > maxY)
				maxY = points[i].y;
		}
		return new Rectangle(minX, minY, maxX - minX, maxY - minY);
	}
	
	/// <summary>
	/// Translates to origin.
	/// </summary>
	/// <returns>The to origin.</returns>
	/// <param name="points">Points.</param>
	public static List<Point> TranslateToOrigin(List<Point> points){
		Point c = Centroid(points);
		List<Point> newpoints = new List<Point>();
		for (var i = 0; i < points.Count; i++)
		{
			double qx = points[i].x - c.x;
			double qy = points[i].y - c.y;
			newpoints.Add(new Point(qx, qy));
		}
		return newpoints;
	}
	/// <summary>
	/// Distances at best angle.
	/// </summary>
	/// <returns>The at best angle.</returns>
	/// <param name="points">Points.</param>
	/// <param name="T">T.</param>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="threshold">Threshold.</param>
	public static double DistanceAtBestAngle(List<Point> points, Template T, double a, double b, double threshold)
	{
		double x1 = Phi * a + (1.0 - Phi) * b;
		double f1 = DistanceAtAngle(points, T, x1);
		double x2 = (1.0 - Phi) * a + Phi * b;
		double f2 = DistanceAtAngle(points, T, x2);
		while (Math.Abs(b - a) > threshold)
		{
			if (f1 < f2)
			{
				b = x2;
				x2 = x1;
				f2 = f1;
				x1 = Phi * a + (1.0 - Phi) * b;
				f1 = DistanceAtAngle(points, T, x1);
			}
			else
			{
				a = x1;
				x1 = x2;
				f1 = f2;
				x2 = (1.0 - Phi) * a + Phi * b;
				f2 = DistanceAtAngle(points, T, x2);
			}
		}
		return Math.Min(f1, f2);
	}
	public static double DistanceAtAngle(List<Point> points, Template T, double theta)
	{
		List<Point> newpoints = RotateBy(points, theta);
		return PathDistance(newpoints, T.Points);
	}
	public static double PathDistance(List<Point> pts1, List<Point> pts2)
	{
		double d = 0.0;
		//Debug.Log (pts1.Count+"____"+pts2.Count);
		for (int i = 0; i < pts1.Count; i++) // assumes pts1.length == pts2.length
			d += Distance(pts1[i], pts2[i]);
		return d / pts1.Count;
	}
}