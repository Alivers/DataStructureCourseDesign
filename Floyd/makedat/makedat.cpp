/**
 * Date: 2019-08-25 16:40:27
 * LastEditors: Aliver
 * LastEditTime: 2019-08-25 18:19:14
 */
#include <iostream>
#include <vector>
#include <time.h>
#include <fstream>
#include <string>
#include <random>
#include <cstring>
#include <limits>
#include <cmath>

using namespace std;

#define OK    1
#define ERROR 0
#define TRUE  1
#define FALSE 0

typedef int Status;

constexpr auto Infinity =  (numeric_limits<double>::max)();

Status Usage()
{
	cout << "--------------------------------------------------------------------------------------" << endl;
	cout << "makedat用法如下：" << endl;
	cout << "1、共五个参数：--help，--vertex [m], --arc [n], --output [*]" << endl;
	cout << "--------------------------------------------------------------------------------------" << endl;

	return OK;
}
template <typename T>
auto random(const T min, const T max, unsigned seed = time(0)) -> decltype(min)
{
    static default_random_engine engine(seed);
    static uniform_real_distribution<T> dist(min, max);
    return dist(engine);
}

Status MakeDataFile( const char *Filename, int vertexNum, int arcNum )
{
    srand( (unsigned int)(time( 0 )) );     //生成随机种子

    ofstream fout( Filename, ios::out );
	if ( !fout.is_open() )
		throw runtime_error( "文件打开失败!" );

    vector<int> vertex(vertexNum);
    vector<vector<double>> arc(vertexNum);
    for (int i = 0; i < arc.size(); ++i) {
        arc[i].resize(vertexNum);
        for (int j = 0; j < arc[i].size(); ++j){
            arc[i][j] = Infinity;
        }
    }
        

    int start, end;

    fout << "[node]" << endl;
    for (int i = 0; i < vertex.size(); ++i)
        fout << i + 1 << "=" << i << endl;

    fout << endl
         << "[line]" << endl;
    for (; arcNum > 0; --arcNum){
        do {
            start = rand() % vertexNum;
            end = rand() % vertexNum;
        } while (start == end || (fabs(arc[start][end] - Infinity) >= 1e-5));
        arc[start][end] = random(0.5, 50.0);
        fout << start + 1 << "," << end + 1 << "," << arc[start][end] << endl;
    }

    fout.close();

    return OK;
}

int main( int argc, char *argv[] )
{
	int arcNum = 20;
    int vertexNum = 10;
    
    char Filename[128] = "a.dat";

    if ( 1 == argc ) {
		cout << "正在生成数据文件......" << endl;
        cout << "顶点数 : " << vertexNum << ", 弧数 : " << arcNum << endl;
        MakeDataFile(Filename, vertexNum, arcNum);
    }
	else {
		for ( int i = 1; i < argc; i++ ) {
			if ( !strcmp( argv[i], "--help" ) ) {
				Usage();
				return 0;
			}
			else if ( !strcmp( argv[i], "--vertex" ) ) {
				if ( !argv[i + 1] )
					break;
				int temp = atoi( argv[i + 1] );
				if ( temp > 0 ) {
					vertexNum = temp;
					++i;
				}
			}
			else if ( !strcmp( argv[i], "--arc" ) ) {
				if ( !argv[i + 1] )
					break;
				int temp = atoi( argv[i + 1] );
				if ( temp > 0 ) {
					arcNum = temp;
					++i;
				}
			}
			else if ( !strcmp( argv[i], "--output" ) ) {
				if ( !argv[i + 1] )
					break;
				strcpy( Filename, argv[i + 1] );
				++i;
			}
		}

		cout << "正在生成数据文件......" << endl;
		cout << "顶点数 : " << vertexNum << ", 弧数 : " << arcNum << endl;
		MakeDataFile(Filename, vertexNum, arcNum);
	}

	return 0;
}