const http = require('http');
const port = 3000;
const server = http.createServer((req, res) => {
  res.statusCode = 200;
  res.setHeader('Content-Type', 'text/html');
  res.end('<h1>POS Express</h1><p>The POS System code is ready.</p>');
});
server.listen(port, () => {
  console.log(`Server running at port ${port}`);
});
