const webpack = require('webpack');
const fs = require('fs');
const path = require('path');

module.exports = {
  configureWebpack: {
    resolve: {
      alias: {
        process: 'process/browser',  // 引入 process polyfill
      }
    },
    plugins: [
      new webpack.ProvidePlugin({
        process: 'process/browser'  // 确保在代码中能够使用 process
      })
    ]
  },
  devServer: {
    https: {
      key: fs.readFileSync(path.join(__dirname, 'ssl/localhost.key')),
      cert: fs.readFileSync(path.join(__dirname, 'ssl/localhost.crt')),
    },
    port: 8080,  // 保持原有端口
    proxy: {
      '/api': {
        target: 'https://localhost:7275',  // 后端的 https 地址
        changeOrigin: true,
        secure: false,  // 允许使用自签名证书
      },
    },
  }
};



