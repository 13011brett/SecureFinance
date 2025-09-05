import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Container, AppBar, Toolbar, Typography, Box } from '@mui/material';
import Dashboard from './components/Dashboard';
import StockData from './components/StockData';
import CryptoData from './components/CryptoData';
import AdminPanel from './components/AdminPanel';
import Navigation from './components/Navigation';

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#00e676',
    },
    secondary: {
      main: '#ff4081',
    },
    background: {
      default: '#0a0e1a',
      paper: '#1a1a2e',
    },
  },
  typography: {
    fontFamily: '"Roboto Mono", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 600,
    },
  },
});

function App() {
  return (
    <ThemeProvider theme={darkTheme}>
      <CssBaseline />
      <Router>
        <Box sx={{ flexGrow: 1 }}>
          <AppBar position="static" sx={{ background: 'linear-gradient(45deg, #1a1a2e 30%, #16213e 90%)' }}>
            <Toolbar>
              <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
                🔒 SecureFinance API
              </Typography>
              <Typography variant="subtitle2" sx={{ opacity: 0.8 }}>
                Enterprise Financial Data Aggregation
              </Typography>
            </Toolbar>
          </AppBar>
          
          <Navigation />
          
          <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/stocks" element={<StockData />} />
              <Route path="/crypto" element={<CryptoData />} />
              <Route path="/admin" element={<AdminPanel />} />
            </Routes>
          </Container>
        </Box>
      </Router>
    </ThemeProvider>
  );
}

export default App;