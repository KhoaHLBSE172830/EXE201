import React from 'react';
import { Outlet } from 'react-router-dom';
import RoleSwitcher from '../../components/common/RoleSwitcher';

/**
 * PublicLayout: Khung bọc cho các trang cộng đồng/khách
 */
export default function PublicLayout() {
  return (
    <div style={{ minHeight: '100vh', background: '#ffffff', color: '#0f172a' }}>
      <Outlet />
      <RoleSwitcher />
    </div>
  );
}
