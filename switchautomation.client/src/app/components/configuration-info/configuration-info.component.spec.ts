import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigurationInfoComponent } from './configuration-info.component';

describe('ConfigurationInfoComponent', () => {
  let component: ConfigurationInfoComponent;
  let fixture: ComponentFixture<ConfigurationInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ConfigurationInfoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfigurationInfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
