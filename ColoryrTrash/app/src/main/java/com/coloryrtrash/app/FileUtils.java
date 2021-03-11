package com.coloryrtrash.app;

import android.content.Context;

import java.io.FileInputStream;
import java.io.FileOutputStream;

public class FileUtils {
    private final Context mContext;

    public FileUtils(Context mContext) {
        this.mContext = mContext;
    }

    /*
     * 这里定义的是一个文件保存的方法，写入到文件中，所以是输出流
     * */
    public void save(String filename, String filecontent) {
        try {
            //这里我们使用私有模式,创建出来的文件只能被本应用访问,还会覆盖原文件哦
            FileOutputStream output = mContext.openFileOutput(filename, Context.MODE_PRIVATE);
            output.write(filecontent.getBytes());  //将String字符串以字节流的形式写入到输出流中
            output.close();         //关闭输出流
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /*
     * 这里定义的是文件读取的方法
     * */
    public String read(String filename) {
        try {
            //打开文件输入流
            FileInputStream input = mContext.openFileInput(filename);
            byte[] temp = new byte[1024];
            StringBuilder sb = new StringBuilder("");
            int len = 0;
            //读取文件内容:
            while ((len = input.read(temp)) > 0) {
                sb.append(new String(temp, 0, len));
            }
            //关闭输入流
            input.close();
            return sb.toString();
        } catch (Exception e) {
            e.printStackTrace();
            return "";
        }
    }
}
