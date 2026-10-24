import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '../services/modules/authService';
import apiClient from '../services/api/apiClient';
import { API_ENDPOINTS } from '../services/api/endpoints';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem('mimo_user');
    return saved ? JSON.parse(saved) : null;
  });

  const [token, setToken] = useState(() => {
    return localStorage.getItem('mimo_token') || null;
  });

  const [loading, setLoading] = useState(false);

  // Tự động đồng bộ trạng thái VIP/Pro và dữ liệu từ Supabase Database khi app khởi chạy hoặc khi có token
  useEffect(() => {
    if (!token) return;
    let isMounted = true;

    apiClient.get(API_ENDPOINTS.AUTH.ME)
      .then((res) => {
        if (!isMounted) return;
        const meData = res?.data?.data || res?.data;
        if (meData) {
          setUser((prev) => {
            const isPro = meData.isPro ?? meData.stats?.isPro ?? prev?.isPro ?? false;
            const cleanPlan = String(meData.planCode || meData.plan || prev?.planCode || (isPro ? 'PREMIUM_6M' : 'FREE'))
              .toUpperCase()
              .replace('MIMO_', '');
            const updated = {
              ...prev,
              ...meData,
              role: prev?.role || 'STUDENT',
              isPro,
              isPlus: isPro || prev?.isPlus,
              plan: cleanPlan,
              planCode: cleanPlan,
              planName: meData.planName || prev?.planName || (isPro ? 'Gói 6 Tháng (Half-Year)' : 'Mimo Free'),
            };
            localStorage.setItem('mimo_user', JSON.stringify(updated));
            return updated;
          });
        }
      })
      .catch((err) => {
        console.warn('Lưu ý đồng bộ me data trong AuthContext:', err?.message);
      });

    return () => {
      isMounted = false;
    };
  }, [token]);

  const login = (userData, userToken) => {
    setUser(userData);
    setToken(userToken);
    localStorage.setItem('mimo_user', JSON.stringify(userData));
    localStorage.setItem('mimo_token', userToken);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    setToken(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!token,
        role: user?.role || null,
        login,
        logout,
        loading,
        setLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
