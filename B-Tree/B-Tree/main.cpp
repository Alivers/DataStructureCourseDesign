#define _CRT_SECURE_NO_WARNINGS
#include "BTree.h"
#include <cstdio>
#include <fstream>
#include <string>
#include <cstring>

void usage(char *prompt = nullptr)
{
    if (prompt)
        cerr << prompt << ",";
    cerr << "---------------------------------------" << endl;
    cerr << "windows : --input *.*" << endl;
    cerr << "linux   : ./* --input *.*" << endl;
    cerr << "其中 *.* 为数据文件名, ./* 表示执行该程序" << endl;
    cerr << "---------------------------------------" << endl;
}

inline void Trim(char* str, unsigned len)
{
    if (!str || !*str)
        return;

    char* p = str, * q = &str[len - 1];

    while (*p && (*p == ' ' || *p == '\t' || *p == '\n' || *p == '\r'))
        ++p;
    while (*q == ' ' || *q == '\t' || *q == '\n' || *q == '\r')
        --q;
    *(++q) = 0;

    memmove(str, p, q - p + 1);

    return;
}

inline void ClearState(ifstream& fin)
{
    if (fin.eof())
        return;
    if (fin.fail())
        fin.clear();
    return;
}

/*
    *读取文件中多余的空格
 */
void ReadSpace(ifstream& fin)
{
    if (fin.eof())
        return;

    ClearState(fin);

    while (fin.peek() == '\t' || fin.peek() == ' ')
        fin.get();

    ClearState(fin);
    return;
}
void ClearLine(ifstream& fin)
{
    if (fin.eof())
        return;

    ClearState(fin);

    fin.ignore(INT16_MAX, '\n');
    if (fin.peek() == '\r' || fin.peek() == '\n')
        fin.get();

    ClearState(fin);

    return;
}

template <typename KeyType>
void Test(BTree<KeyType> &t, ifstream &fin)
{
    const char* ord = "m";
    const char* ins = "insert";
    const char* del = "delete";
    const char* sea = "find";

    bool OrderSeted = false;

    char OpeCmd[16];
    
    while (!fin.eof()) {

        if (!(fin.getline(OpeCmd, sizeof(OpeCmd), ' '))) {
            ClearLine(fin);
            continue;
        }
        Trim(OpeCmd, strlen(OpeCmd));

        if (OpeCmd[0] == '#') {
            ClearLine(fin);
            continue;
        }

        if (!strncmp(OpeCmd, ord, 1)) {
            int m;
            if (!(fin >> m)) {
                ClearLine(fin);
                continue;
            }
            if (!OrderSeted) {
                BTree<KeyType>::SetOrder(m);
                OrderSeted = true;
            }
            ClearLine(fin);
        }
        else if (!strncmp(OpeCmd, ins, 6)) {
            int key;
            if (!(fin >> key)) {
                ClearLine(fin);
                continue;
            }
            if (t.Insert(key) == BTree<KeyType>::EXISTED) {
                cout << "关键字 " << key << " 已存在，插入失败" << endl;
            }
            else {
                t.FormatPrint((string("插入") + to_string(key) + string("后的B-Tree")).c_str());
            }
            ClearLine(fin);
        }
        else if (!strncmp(OpeCmd, del, 6)) {
            int key;
            if (!(fin >> key)) {
                ClearLine(fin);
                continue;
            }
            if (t.Delete(key) == BTree<KeyType>::NOTFOUND) {
                cout << "关键字 " << key << " 不存在，删除失败" << endl;
            }
            else {
                t.FormatPrint((string("删除") + to_string(key) + string("后的B-Tree")).c_str());
            }
            ClearLine(fin);
        }
        else if (!strncmp(OpeCmd, sea, 4)) {
            int key;
            if (!(fin >> key)) {
                ClearLine(fin);
                continue;
            }
            if (!t.Search(key).tag) {
                cout << "关键字 " << key << " 不存在，查找失败" << endl;
            }
            else {
                t.SearchPath((string("查找") + to_string(key) + string("的路径")).c_str());
                cout << endl;
            }
            ClearLine(fin);
        }
        else {
            ClearLine(fin);
        }
    }
}

int main(int argc, char* argv[])
{
    if (argc < 3 || (argc >= 3 && strcmp(argv[1], "--input") != 0)) {
        usage(nullptr);
        return 0;
    }
    ifstream fin(argv[2], ios::in | ios::binary);
    if (!fin.is_open()) {
        cerr << "未找到数据文件" << endl;
        return 0;
    }

    BTree<int> t;
    Test(t, fin);

    fin.close();
    return 0;
}